/* Filename:    SendCMove.cs
 *
 * Author:      Rob Holme (rob@holme.com.au)
 *
 * Notes:       Implements a powershell CmdLet to send a DICOM C-MOVE to instruct a remote host
 *              to send images to a specified move destination AE title.
 *
 */


namespace DicomTools {
	using System;
	using System.Collections.Generic;
	using System.Management.Automation;
	using System.Threading;
	using FellowOakDicom;
	using FellowOakDicom.Network;
	using FellowOakDicom.Network.Client;

	[Cmdlet(VerbsCommunications.Send, "CMove")]
	public class SendCMove : PSCmdlet {

		private string dicomRemoteHost;
		private int dicomRemoteHostPort;
		private string callingDicomAeTitle = "DICOMTOOLS-SCU";
		private string calledDicomAeTitle = "ANY-SCP";
		private string moveDestinationAeTitle;
		private string studyInstanceUID = "";
		private string seriesInstanceUID = "";
		private string sopInstanceUID = "";
		private bool useTls = false;
		private int timeoutInSeconds = 60;

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

		// The move destination AE title (where images will be sent)
		[Parameter(
			Mandatory = true,
			Position = 5,
			HelpMessage = "The AE title of the destination to send images to"
		)]
		public string MoveDestination {
			get { return this.moveDestinationAeTitle; }
			set { this.moveDestinationAeTitle = value; }
		}

		// Study Instance UID to retrieve
		[Parameter(
			Mandatory = true,
			Position = 6,
			HelpMessage = "The Study Instance UID to retrieve",
			ParameterSetName = "Study"
		)]
		public string StudyInstanceUID {
			get { return this.studyInstanceUID; }
			set { this.studyInstanceUID = value; }
		}

		// Series Instance UID to retrieve
		[Parameter(
			Mandatory = true,
			Position = 6,
			HelpMessage = "The Series Instance UID to retrieve",
			ParameterSetName = "Series"
		)]
		public string SeriesInstanceUID {
			get { return this.seriesInstanceUID; }
			set { this.seriesInstanceUID = value; }
		}

		// SOP Instance UID to retrieve a single image
		[Parameter(
			Mandatory = true,
			Position = 6,
			HelpMessage = "The SOP Instance UID to retrieve",
			ParameterSetName = "Image"
		)]
		public string SOPInstanceUID {
			get { return this.sopInstanceUID; }
			set { this.sopInstanceUID = value; }
		}

		// Use TLS for the connection
		[Parameter(
			Mandatory = false,
			Position = 7,
			HelpMessage = "Use TLS to secure the connection"
		)]
		public SwitchParameter UseTLS {
			get { return this.useTls; }
			set { this.useTls = value; }
		}

		// timeout waiting for a response from the server.
		[Parameter(
			Mandatory = false,
			Position = 8,
			HelpMessage = "The timeout in seconds to wait for a response"
		)]
		[ValidateRange(1, 600)]
		public int Timeout {
			get { return this.timeoutInSeconds; }
			set { this.timeoutInSeconds = value; }
		}

		/// <summary>
		/// Process all C-MOVE requests
		/// </summary>
		protected override void ProcessRecord() {

			// write connection details if -Verbose switch supplied
			WriteVerbose($"Hostname:              {dicomRemoteHost}");
			WriteVerbose($"Port:                  {dicomRemoteHostPort}");
			WriteVerbose($"Calling AE Title:      {callingDicomAeTitle}");
			WriteVerbose($"Called AE Title:       {calledDicomAeTitle}");
			WriteVerbose($"Move Destination:      {moveDestinationAeTitle}");
			WriteVerbose($"Use TLS:               {useTls}");
			WriteVerbose($"Timeout:               {timeoutInSeconds}");

			var verboseList = new List<string>();

			try {
				// cancel token to cancel the request after a timeout
				CancellationTokenSource sourceCancelToken = new CancellationTokenSource();
				CancellationToken cancelToken = sourceCancelToken.Token;

				// create new DICOM client
				var client = DicomClientFactory.Create(dicomRemoteHost, dicomRemoteHostPort, useTls, callingDicomAeTitle, calledDicomAeTitle);
				client.ServiceOptions.LogDimseDatasets = false;
				client.ServiceOptions.LogDataPDUs = false;
				client.ServiceOptions.RequestTimeout = new TimeSpan(0, 0, timeoutInSeconds);
				client.NegotiateAsyncOps();

				// determine the query retrieve level and create the C-MOVE request
				DicomCMoveRequest cMoveRequest;
				if (sopInstanceUID.Length > 0) {
					cMoveRequest = new DicomCMoveRequest(moveDestinationAeTitle, studyInstanceUID, seriesInstanceUID, sopInstanceUID);
					WriteVerbose($"C-MOVE Level: IMAGE, SOP Instance UID: {sopInstanceUID}");
				}
				else if (seriesInstanceUID.Length > 0) {
					cMoveRequest = new DicomCMoveRequest(moveDestinationAeTitle, studyInstanceUID, seriesInstanceUID);
					WriteVerbose($"C-MOVE Level: SERIES, Series Instance UID: {seriesInstanceUID}");
				}
				else {
					cMoveRequest = new DicomCMoveRequest(moveDestinationAeTitle, studyInstanceUID);
					WriteVerbose($"C-MOVE Level: STUDY, Study Instance UID: {studyInstanceUID}");
				}

				// store the final result
				SendCMoveResult cMoveResult = null;

				// event handler - C-MOVE response received
				cMoveRequest.OnResponseReceived += (request, response) => {
					verboseList.Add($"C-MOVE Response: {response.Status} (Remaining: {response.Remaining}, Completed: {response.Completed}, Failed: {response.Failures}, Warning: {response.Warnings})");
					cMoveResult = new SendCMoveResult(
						response.Status.ToString(),
						(int)response.Completed,
						(int)response.Failures,
						(int)response.Remaining,
						(int)response.Warnings,
						moveDestinationAeTitle
					);
				};

				// event handler - client association rejected by server
				client.AssociationRejected += (sender, eventArgs) => {
					verboseList.Add($"Association was rejected. Reason:{eventArgs.Reason}");
				};

				// event handler - client association accepted by server
				client.AssociationAccepted += (sender, eventArgs) => {
					verboseList.Add($"Association was accepted by:{eventArgs.Association.RemoteHost}");
				};

				// add the C-MOVE request to the client
				client.AddRequestAsync(cMoveRequest);

				// send an async request, wait for response.
				// cancel after period specified by -Timeout parameter
				sourceCancelToken.CancelAfter(timeoutInSeconds * 1000);
				var task = client.SendAsync(cancelToken);
				task.Wait();

				// write verbose logging from the async event handlers (cant write to pwsh host from another thread)
				verboseList.Reverse();
				foreach (string verboseString in verboseList) {
					WriteVerbose(verboseString);
				}

				// check to see if the task timed out, otherwise return results.
				if (cancelToken.IsCancellationRequested) {
					WriteWarning($"The C-MOVE request timed out (timeout set to {timeoutInSeconds} seconds). Use -Timeout to increase duration.");
				}
				else {
					if (cMoveResult != null) {
						WriteObject(cMoveResult);
					}
					else {
						WriteWarning("No response was received from the server.");
					}
				}
			}
			catch (Exception e) {
				// typically network connection errors will trigger exceptions (remote host unreachable, TLS not supported, etc)
				WriteWarning($"An Issue occurred: {e.InnerException.Message}");
				WriteWarning("Use -Debug switch for full exception message.");
				WriteDebug($"Exception: -> {e}");
			}
		}
	}
}
