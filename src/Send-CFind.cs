/* Filename:    SendCFind.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Date:        16/08/2021
 * 
 * Notes:       Implements a powershell CmdLet to send a DICOM C-FIND to a remote host and display results returned.
 * 
 */
 	

namespace DicomTools
{
	using System;
	using System.Collections.Generic;
	using System.Management.Automation;
	using Dicom;
	using Dicom.Network;
		
	[Cmdlet(VerbsCommunications.Send, "CFind")]
    public class SendCFind : PSCmdlet
    {
		
		 private string dicomRemoteHost;
		 private int dicomRemoteHostPort;
		 private string callingDicomAeTitle = "DICOMTOOLS";
		 private string calledDicomAeTitle = "ANY-SCP";
		 private string patientName;
		 private bool useTls = false;
		 private string responseStatus;
		 private string verboseString;
		 private int timeoutInSeconds = 5;


        // Hostname or IP Address of DICOM service
        [Parameter(
            Mandatory = true,
			ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            Position = 1,
            HelpMessage = "Hostname or IP Address of DICOM service"
        )]
        [Alias("IPAddress")]
        public string HostName
        {
            get { return this.dicomRemoteHost; }
            set { this.dicomRemoteHost = value; }
        }
               
		// The remote port number of the DICOM service 
        [Parameter(
            Mandatory = true,
            Position = 2,
            HelpMessage = "Port number of remote DICOM service"
        )]
		[ValidateRange(1,65535)]
        public int Port
        {
            get { return this.dicomRemoteHostPort; }
            set { this.dicomRemoteHostPort = value; }
        }

		// The client calling AE title
        [Parameter(
            Mandatory = false,
            Position = 3,
            HelpMessage = "The client calling AE title"
        )]
        public string LocalAETitle
        {
            get { return this.callingDicomAeTitle; }
            set { this.callingDicomAeTitle = value; }
        }

		// The server called AE title
        [Parameter(
            Mandatory = false,
            Position = 4,
            HelpMessage = "The server called AE title"
        )]
        public string RemoteAETitle
        {
            get { return this.calledDicomAeTitle; }
            set { this.calledDicomAeTitle = value; }
        }   

		// Patient name
        [Parameter(
            Mandatory = false,
            Position = 4,
            HelpMessage = "The patient name to search for"
        )]
        public string PatientName
        {
            get { return this.patientName; }
            set { this.patientName = value; }
        }   

		// Use TLS for the connection
        [Parameter(
            Mandatory = false,
            Position = 5,
            HelpMessage = "Use TLS to secure the connection"
        )]
        public SwitchParameter UseTLS
        {
            get { return this.useTls; }
            set { this.useTls = value; }
        }   

		// The timeout waiting for a response from the DICOM service
        [Parameter(
            Mandatory = false,
            Position = 6,
            HelpMessage = "Timeout (in seconds) to wait for a response from the DICOM service"
        )]
		[ValidateRange(1,20)]
        public int Timeout
        {
            get { return this.timeoutInSeconds; }
            set { this.timeoutInSeconds = value; }
        }
        
        /// <summary>
        /// get the HL7 item provided via the cmdlet parameter HL7ItemPosition
        /// </summary>
        protected override void ProcessRecord()
        {
			WriteVerbose("Hostname:          " + dicomRemoteHost);
			WriteVerbose("Port:              " + dicomRemoteHostPort);
			WriteVerbose("Calling AE Title:  " + callingDicomAeTitle);
			WriteVerbose("Called AE Title:   " + calledDicomAeTitle);
			WriteVerbose("Use TLS:           " + useTls);
			WriteVerbose("Timeout (seconds): " + timeoutInSeconds);
			
			try
            {
				// create new DICOM client. Set timeout option based on -Timeout parameter use provides (defaults to 5 secs)
				var client = new Dicom.Network.Client.DicomClient(dicomRemoteHost, dicomRemoteHostPort, useTls, callingDicomAeTitle, calledDicomAeTitle);
				client.Options = new Dicom.Network.DicomServiceOptions();
				client.Options.RequestTimeout = new TimeSpan(0,0,timeoutInSeconds);
				client.NegotiateAsyncOps();
				var cFindRequest = new DicomCFindRequest(DicomQueryRetrieveLevel.Study);

				// The attributes to be returned in the result need to be specified with empty parameters
// POPULATE THESE FROM PARAMETERS (that default to "" if not suppplied)
                cFindRequest.Dataset.AddOrUpdate(DicomTag.PatientName, "");
                cFindRequest.Dataset.AddOrUpdate(DicomTag.PatientID, "");
				cFindRequest.Dataset.AddOrUpdate(DicomTag.PatientBirthDate, "");
				cFindRequest.Dataset.AddOrUpdate(DicomTag.PatientSex, "");
                cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyDate, "");
                cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, "");

				// Filter the search on Patient Name 
                cFindRequest.Dataset.AddOrUpdate(DicomTag.PatientName, patientName);

				// list of studies returned
                // var studyUids = new List<string>();
				var cFindResultList = new List<SendCFindResult>();
               
                // Specify the encoding of the retrieved results
                // here the character set is 'Latin alphabet No. 1'
                cFindRequest.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");

				// event handler - response received from C-Find request
				cFindRequest.OnResponseReceived += (request, response) => {
                if (response.Status == DicomStatus.Pending)
                {
                    var patientName = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);
					var patientID = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
					var patientDOB = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, string.Empty);
					var patientSex = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty);
                    var studyDate = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyDate, string.Empty);
                    var studyUID = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
					cFindResultList.Add(new SendCFindResult(patientName, patientID, patientDOB, patientSex, studyDate, studyUID));
                }

                if (response.Status == DicomStatus.Success)
                {
// log this to a thread safe queue, then have writeversose read from the queue.
//                    LogToDebugConsole(response.Status.ToString());
                }

				};

				// event handler - client association rejected by server
				client.AssociationRejected += (sender, eventArgs) => {
					responseStatus = $"Association was rejected. Reason:{eventArgs.Reason}";
            	};

				// event handler - client association accepted by server
            	client.AssociationAccepted += (sender, eventArgs) => {
                	verboseString += $"Association was accepted by:{eventArgs.Association.RemoteHost}";
            	};

				// add the C-FIND request to the client
                client.AddRequestAsync(cFindRequest);
				// send an async request, wait for response (Powershell output can't be from a thread). 
				var task = client.SendAsync();
				task.Wait();
				if (verboseString.Length > 0) {
					WriteVerbose(verboseString);
				}
				if (responseStatus.Length > 0) {
					WriteWarning(responseStatus);
				}

				// write the results to the pipeline
				WriteObject(cFindResultList);
            }
            catch (Exception e)
            {	
				// typically network connection errors will trigger exceptions (remote host unreachable) 
				var result = new SendCEchoResult(dicomRemoteHost, dicomRemoteHostPort, "Failed: " + e.InnerException.Message, 0);
				WriteObject(result);
                //In real life, do something about this exception
                WriteDebug($"Exception: -> {e}");
            }
        }   
	}
}