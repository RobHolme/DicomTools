namespace DicomTools
{
	using System;
    using System.Diagnostics;
	using System.Management.Automation;
	using Dicom.Network;
//	using Dicom.Network.Client;

		
	[Cmdlet(VerbsCommunications.Send, "CEcho")]
    public class SendCEcho : PSCmdlet
    {
		
		 private string dicomRemoteHost;
		 private int dicomRemoteHostPort;
		 private string callingDicomAeTitle = "DICOMTOOLS";
		 private string calledDicomAeTitle = "ANY-SCP";
		 private bool useTls = false;
		 private string responseStatus = "Failed";
		 private string verboseString;


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

		// The server called AE title
        [Parameter(
            Mandatory = false,
            Position = 5,
            HelpMessage = "Use TLS to secure the connection"
        )]
        public bool UseTLS
        {
            get { return this.useTls; }
            set { this.useTls = value; }
        }   
        
        /// <summary>
        /// get the HL7 item provided via the cmdlet parameter HL7ItemPosition
        /// </summary>
        protected override void ProcessRecord()
        {
			WriteVerbose("Hostname:        " + dicomRemoteHost);
			WriteVerbose("Port:            " + dicomRemoteHostPort);
			WriteVerbose("Calling AE Title:" + callingDicomAeTitle);
			WriteVerbose("Called AE Title: " + calledDicomAeTitle);
			WriteVerbose("Use TLS:         " + useTls);
			
			try
            {
				var client = new Dicom.Network.Client.DicomClient(dicomRemoteHost, dicomRemoteHostPort, useTls, callingDicomAeTitle, calledDicomAeTitle,5000,5000,5000,5000);
				var cEchoRequest = new DicomCEchoRequest();

				cEchoRequest.OnResponseReceived += (request, response) =>
				{
					responseStatus = response.Status.ToString();
				};

				client.AssociationRejected += (sender, eventArgs) =>
            	{
					responseStatus = $"Association was rejected. Reason:{eventArgs.Reason}";
            	};


            	client.AssociationAccepted += (sender, eventArgs) =>
           		{
                	verboseString += $"Association was accepted by:{eventArgs.Association.RemoteHost}";
            	};

				// send an async request, wait for response (Powershell output can't be from a thread).
				Stopwatch timer = new Stopwatch() ; 
				client.AddRequestAsync(cEchoRequest);
				timer.Start();
				var task = client.SendAsync();
				task.Wait();
				timer.Stop();
				if (verboseString.Length > 0) {
					WriteVerbose(verboseString);
				}

				// write the results to the pipeline
				var result = new SendCEchoResult(dicomRemoteHost, dicomRemoteHostPort, responseStatus, timer.ElapsedMilliseconds);
				WriteObject(result);

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