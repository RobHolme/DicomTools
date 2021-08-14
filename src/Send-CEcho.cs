namespace DicomTools
{
	using System;
    using System.Diagnostics;
	using System.Management.Automation;
	using Dicom.Network;
	using Dicom.Network.Client;

		
	[Cmdlet(VerbsCommunications.Send, "CEcho")]
    public class SendCEcho : PSCmdlet
    {
		
		 private string dicomRemoteHost;
		 private int dicomRemoteHostPort;
		 private string callingDicomAeTitle = "SCU";
		 private string calledDicomAeTitle = "ANY-SCP";
		 private bool useTls = false;


        // Hostname or IP Address of DICOM service
        [Parameter(
            Mandatory = true,
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
				   var result = new SendCEchoResult(dicomRemoteHost, dicomRemoteHostPort, response.Status.ToString(), 0);
				   WriteObject(result);
				};

				client.AddRequestAsync(cEchoRequest);
				//client.AddRequest(cEchoRequest);
				client.SendAsync();

            }
            catch (Exception e)
            {
                //In real life, do something about this exception
                WriteWarning($"Error occured during DICOM verification request -> {e.StackTrace}");
            }
        }   
	}
}