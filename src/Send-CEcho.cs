/* Filename:    SendCEcho.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Date:        14/08/2021
 * 
 * Notes:       Implements a powershell CmdLet to send a DICOM C-ECHO to a remote host
 * 
 */


namespace DicomTools {
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Diagnostics;
	using System.Management.Automation;
	using FellowOakDicom.Network;
	using FellowOakDicom.Network.Client;
	using FellowOakDicom.Log;

	[Cmdlet(VerbsCommunications.Send, "CEcho")]
	public class SendCEcho : PSCmdlet {

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
		public string HostName {
			get { return this.dicomRemoteHost; }
			set { this.dicomRemoteHost = value; }
		}

		// The remote port number of the DICOM service 
		[Parameter(
			Mandatory = true,
			Position = 2,
			HelpMessage = "Port number of remote DICOM service"
		)]
		[ValidateRange(1, 65535)]
		public int Port {
			get { return this.dicomRemoteHostPort; }
			set { this.dicomRemoteHostPort = value; }
		}

		// The client calling AE title
		[Parameter(
			Mandatory = false,
			Position = 3,
			HelpMessage = "The client calling AE title"
		)]
		[Alias("CallingAETitle")]
		public string LocalAETitle {
			get { return this.callingDicomAeTitle; }
			set { this.callingDicomAeTitle = value; }
		}

		// The server called AE title
		[Parameter(
			Mandatory = false,
			Position = 4,
			HelpMessage = "The server called AE title"
		)]
		[Alias("CalledAETitle")]
		public string RemoteAETitle {
			get { return this.calledDicomAeTitle; }
			set { this.calledDicomAeTitle = value; }
		}

		// Use TLS for the connection
		[Parameter(
			Mandatory = false,
			Position = 5,
			HelpMessage = "Use TLS to secure the connection"
		)]
		public SwitchParameter UseTLS {
			get { return this.useTls; }
			set { this.useTls = value; }
		}

		// The timeout waiting for a response from the DICOM service
		[Parameter(
			Mandatory = false,
			Position = 6,
			HelpMessage = "Timeout (in seconds) to wait for a response from the DICOM service"
		)]
		[ValidateRange(1, 60)]
		public int Timeout {
			get { return this.timeoutInSeconds; }
			set { this.timeoutInSeconds = value; }
		}

		/// <summary>
		/// get the HL7 item provided via the cmdlet parameter HL7ItemPosition
		/// </summary>
		protected override void ProcessRecord() {
			WriteVerbose("Hostname:          " + dicomRemoteHost);
			WriteVerbose("Port:              " + dicomRemoteHostPort);
			WriteVerbose("Calling AE Title:  " + callingDicomAeTitle);
			WriteVerbose("Called AE Title:   " + calledDicomAeTitle);
			WriteVerbose("Use TLS:           " + useTls);
			WriteVerbose("Timeout (seconds): " + timeoutInSeconds);

			var verboseList = new List<string>();

			try {
				// cancel token to cancel the request after a timeout
				CancellationTokenSource sourceCancelToken = new CancellationTokenSource();
				CancellationToken cancelToken = sourceCancelToken.Token;

				// create new DICOM client. Set timeout option based on -Timeout parameter use provides (defaults to 5 secs)
				var client = DicomClientFactory.Create(dicomRemoteHost, dicomRemoteHostPort, useTls, callingDicomAeTitle, calledDicomAeTitle);
				client.ServiceOptions.LogDimseDatasets = false;
				client.ServiceOptions.LogDataPDUs = false;
				client.ServiceOptions.RequestTimeout = new TimeSpan(0, 0, timeoutInSeconds);
				// suppress console logging unless in debug mode 
//				if (!this.MyInvocation.BoundParameters.ContainsKey("Debug")) {
//					client.Logger = new NullLogger();
//				}
				var cEchoRequest = new DicomCEchoRequest();

				// event handler - response received from C-ECHO request
				cEchoRequest.OnResponseReceived += (request, response) => {
					verboseList.Add("Response received");
					if (response != null) {
						responseStatus = response.Status.ToString();
					}
				};

				// event handler - client association rejected by server
				client.AssociationRejected += (sender, eventArgs) => {
					verboseList.Add("Association was rejected");
					responseStatus = $"Association was rejected. Reason:{eventArgs.Reason}";
				};

				// event handler - client association accepted by server
				client.AssociationAccepted += (sender, eventArgs) => {
					verboseList.Add($"Association was accepted by:{eventArgs.Association.RemoteHost}");
				};

				// send an async request, wait for response (Powershell output can't be from a thread). 
				// cancel request after -Timeout period
				// Record connection time. 				
				Stopwatch timer = new Stopwatch();
				client.AddRequestAsync(cEchoRequest);
				timer.Start();
				sourceCancelToken.CancelAfter(timeoutInSeconds * 1000);
				var task = client.SendAsync(cancelToken);
				task.Wait();
				timer.Stop();

				// write verbose logging from the async event handlers (cant write to pwsh host from anther thread)
				verboseList.Reverse();
				foreach (string verboseString in verboseList) {
					WriteVerbose(verboseString);
				}

				// check to see if the task timed out. Write the results to the pipeline
				if (cancelToken.IsCancellationRequested) {
					WriteVerbose($"The C-ECHO query timed out (timeout set to {timeoutInSeconds} seconds). Use -Timeout to increase duration.");
					var result = new SendCEchoResult(dicomRemoteHost, dicomRemoteHostPort, "Failed: Connection timeout", timer.ElapsedMilliseconds);
					WriteObject(result);
				}
				else {
					var result = new SendCEchoResult(dicomRemoteHost, dicomRemoteHostPort, responseStatus, timer.ElapsedMilliseconds);
					WriteObject(result);
				}
			}
			catch (Exception e) {
				// typically network connection errors will trigger exceptions (remote host unreachable) 
				var result = new SendCEchoResult(dicomRemoteHost, dicomRemoteHostPort, "Failed: " + e.InnerException.Message, 0);
				WriteObject(result);
				//In real life, do something about this exception
				WriteDebug($"Exception: -> {e}");
			}
		}

	}
}