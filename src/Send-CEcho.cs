/* Filename:    SendCEcho.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Date:        14/08/2021
 * 
 * Notes:       Implements a powershell CmdLet to send a DICOM C-ECHO to a remote host
 * 
 */
 	

namespace DicomTools
{
	using System;
    using System.Diagnostics;
	using System.Management.Automation;
	using Dicom.Network;
		
	[Cmdlet(VerbsCommunications.Send, "CEcho")]
    public class SendCEcho : PSCmdlet
    {
		
		 private string dicomRemoteHost;
		 private int dicomRemoteHostPort;
		 private string callingDicomAeTitle = "DICOMTOOLS-SCU";
		 private string calledDicomAeTitle = "ANY-SCP";
		 private bool useTls = false;
		 private string responseStatus = "Failed";
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
			
			var verboseList = new List<string>();

			try
            {
				// create new DICOM client. Set timeout option based on -Timeout parameter use provides (defaults to 5 secs)
				var client = new Dicom.Network.Client.DicomClient(dicomRemoteHost, dicomRemoteHostPort, useTls, callingDicomAeTitle, calledDicomAeTitle);
				client.Options = new Dicom.Network.DicomServiceOptions();
				client.Options.RequestTimeout = new TimeSpan(0,0,timeoutInSeconds);
				var cEchoRequest = new DicomCEchoRequest();
				

				// event handler - response received from C-ECHO request
				cEchoRequest.OnResponseReceived += (request, response) => {
					verboseList.add("Response received");
					if (response != null) {
						responseStatus = response.Status.ToString();
					}
				};

				// event handler - client association rejected by server
				client.AssociationRejected += (sender, eventArgs) => {
					verboseList.add("Association was rejected");
					responseStatus = $"Association was rejected. Reason:{eventArgs.Reason}";
            	};

				// event handler - client association accepted by server
            	client.AssociationAccepted += (sender, eventArgs) => {
                	verboseList.add($"Association was accepted by:{eventArgs.Association.RemoteHost}");
            	};

				// send an async request, wait for response (Powershell output can't be from a thread). 
				// Record connection time. 				
				Stopwatch timer = new Stopwatch() ; 
				client.AddRequestAsync(cEchoRequest);
				timer.Start();
				var task = client.SendAsync();
				task.Wait(timeoutInSeconds*1000);
				timer.Stop();
				foreach (string verboseString in verboseList.Reverse()) {
					WriteVerbose(verboseString);
				}

				// write the results to the pipeline
				var result = new SendCEchoResult(dicomRemoteHost, dicomRemoteHostPort, responseStatus, timer.ElapsedMilliseconds);
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