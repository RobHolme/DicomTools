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
	using System.Text.RegularExpressions;
	using Dicom;
	using Dicom.Network;
		
	[Cmdlet(VerbsCommunications.Send, "CFind")]
    public class SendCFind : PSCmdlet
    {
		
		private string dicomRemoteHost;
		private int dicomRemoteHostPort;
		private string callingDicomAeTitle = "DICOMTOOLS-SCU";
		private string calledDicomAeTitle = "ANY-SCP";
		private string patientName = "";
		private string patientID = "";
		private string studyID = "";
		private string modalityType = "";
		private string studyDateStart = "";
		private string studyDateEnd = "";
		private bool useTls = false;
		private string responseStatus = "";
		private string verboseString = "";
		private bool abortProcessing = false;
		private int timeoutInSeconds = 20;

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
            Mandatory = true,
            Position = 4,
            HelpMessage = "The patient name to search for",
			ParameterSetName = "PatientDemographics"
        )]
        public string PatientName
        {
            get { return this.patientName; }
            set { this.patientName = value; }
        }   

		// Patient ID
        [Parameter(
            Mandatory = true,
            Position = 4,
            HelpMessage = "The patient name to search for",
			ParameterSetName = "PatientID"
        )]
        public string PatientID
        {
            get { return this.patientID; }
            set { this.patientID = value; }
        }  

		// Study ID
        [Parameter(
            Mandatory = true,
            Position = 4,
            HelpMessage = "The Study ID to search for",
			ParameterSetName = "StudyID"
        )]
        public string StudyID
        {
            get { return this.studyID; }
            set { this.studyID = value; }
        }

		// Constrain results to specific modalities
        [Parameter(
            Mandatory = false,
            Position = 5,
            HelpMessage = "Constrain results to specific modalities"
        )]
        public string Modality
        {
            get { return this.modalityType; }
            set { this.modalityType = value; }
        } 

		// Search for studies acquired on or after this date
        [Parameter(
            Mandatory = false,
            Position = 5,
            HelpMessage = "Include studies from or after this date YYYYMMDD"
        )]
        public string StartDate
        {
            get { return this.studyDateStart; }
            set { this.studyDateStart = value; }
        } 

		// Search for studies acquired on or before this date
        [Parameter(
            Mandatory = false,
            Position = 6,
            HelpMessage = "Include studies from or after this date YYYYMMDD"
        )]
        public string EndDate
        {
            get { return this.studyDateEnd; }
            set { this.studyDateEnd = value; }
        } 

		// Use TLS for the connection
        [Parameter(
            Mandatory = false,
            Position = 7,
            HelpMessage = "Use TLS to secure the connection"
        )]
        public SwitchParameter UseTLS
        {
            get { return this.useTls; }
            set { this.useTls = value; }
        }   
		
		// timeout waiting for a response from the server.
        [Parameter(
            Mandatory = false,
            Position = 8,
            HelpMessage = "The timeout in seconds to wait for a response"
        )]
        public int Timeout
        {
            get { return this.timeoutInSeconds; }
            set { this.timeoutInSeconds = value; }
        }  

		/// <summary>
        /// begin processing
        /// </summary>
		protected override void BeginProcessing() {

			// validate the dates are entered in the correct format
			if (studyDateStart.Length > 0) {
				if (!Regex.IsMatch(this.studyDateStart, "^(19)|(20)[0-9]{6}$")) {
					WriteWarning("StartDate must be in the format YYYYMMDD");
		  			abortProcessing = true;
					return;
				}
			}
			if (studyDateEnd.Length > 0) {
				if (!Regex.IsMatch(this.studyDateEnd, "^(19)|(20)[0-9]{6}$")) {
					WriteWarning("EndDate must be in the format YYYYMMDD");
					abortProcessing = true;
					return;
				}
			}
		}
	        
        /// <summary>
        /// Process all C-FIND requests
        /// </summary>
        protected override void ProcessRecord()
        {
			if (abortProcessing) {
				return;
			}

			// write connection details if -Verbose switch supplied
			WriteVerbose("Hostname:          " + dicomRemoteHost);
			WriteVerbose("Port:              " + dicomRemoteHostPort);
			WriteVerbose("Calling AE Title:  " + callingDicomAeTitle);
			WriteVerbose("Called AE Title:   " + calledDicomAeTitle);
			WriteVerbose("Use TLS:           " + useTls);
			
			try
            {
				// create new DICOM client. Set timeout option based on -Timeout parameter use provides (defaults to 5 secs)
				var client = new Dicom.Network.Client.DicomClient(dicomRemoteHost, dicomRemoteHostPort, useTls, callingDicomAeTitle, calledDicomAeTitle);
				client.Options = new Dicom.Network.DicomServiceOptions();
				client.Options.RequestTimeout = new TimeSpan(0,0,timeoutInSeconds);
				client.NegotiateAsyncOps();
				var cFindRequest = new DicomCFindRequest(DicomQueryRetrieveLevel.Study);

				// The attributes to be returned in the result need to be specified with empty parameters.
				// Populate attributes with values if to be used in the search query.
                cFindRequest.Dataset.AddOrUpdate(DicomTag.PatientName, patientName);
                cFindRequest.Dataset.AddOrUpdate(DicomTag.PatientID, patientID);
				cFindRequest.Dataset.AddOrUpdate(DicomTag.PatientBirthDate, "");
				cFindRequest.Dataset.AddOrUpdate(DicomTag.PatientSex, "");
				cFindRequest.Dataset.AddOrUpdate(DicomTag.ModalitiesInStudy, modalityType);
                cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyInstanceUID, studyID);
				if ((studyDateStart.Length > 0) | (studyDateEnd.Length > 0)) {
                	cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyDate, $"{studyDateStart}-{studyDateEnd}");
				}
				else {
                	cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyDate, "");
				}

                // The encoding of the results ('ISO_IR 100' is 'Latin Alphabet No. 1').  
				// http://dicom.nema.org/dicom/2013/output/chtml/part02/sect_D.6.html
                cFindRequest.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");
				
				// list to store the results returned
				var cFindResultList = new List<SendCFindResult>();

				// event handler - response received from C-Find request
				cFindRequest.OnResponseReceived += (request, response) => {
                	if (response.Status == DicomStatus.Pending) {
                    	var responsePatientName = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);
						var responsePatientID = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
						var responsePatientDOB = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, string.Empty);
						var responsePatientSex = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty);
						var responseModality = response.Dataset.GetSingleValueOrDefault(DicomTag.ModalitiesInStudy, string.Empty);
						var responseStudyDate = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyDate, string.Empty);
                    	var responseStudyUID = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
						cFindResultList.Add(new SendCFindResult(responsePatientName, responsePatientID, responsePatientDOB, responsePatientSex, responseModality, responseStudyDate, responseStudyUID));
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
				task.Wait(timeoutInSeconds);
				
				// write verbose on association accepted, or warning if association was rejected. 
				// can't write to pipeline directly from the event handlers as it must be from the main thread.
				if (verboseString.Length > 0) {
					WriteVerbose(verboseString);
				}
				if (responseStatus.Length > 0) {
					WriteWarning(responseStatus);
				}

				// write the C-FIND results to the pipeline
				WriteObject(cFindResultList);
            }
            catch (Exception e)
            {	
				// typically network connection errors will trigger exceptions (remote host unreachable, TLS not supported, etc) 
				WriteWarning($"An Issue occurred: {e.InnerException.Message}");
				WriteWarning("Use -Debug switch for full exception message.");
                WriteDebug($"Exception: -> {e}");
            }
        }   
	}
}