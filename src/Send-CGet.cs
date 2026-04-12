/* Filename:    SendCGet.cs
 *
 * Author:      Rob Holme (rob@holme.com.au)
 *
 * Notes:       Implements a powershell CmdLet to send a DICOM C-GET to retrieve images from a remote host.
 *
 */


namespace DicomTools {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Management.Automation;
	using System.Threading;
	using FellowOakDicom;
	using FellowOakDicom.Network;
	using FellowOakDicom.Network.Client;

	[Cmdlet(VerbsCommunications.Send, "CGet")]
	public class SendCGet : PSCmdlet {

		private string dicomRemoteHost;
		private int dicomRemoteHostPort;
		private string callingDicomAeTitle = "DICOMTOOLS-SCU";
		private string calledDicomAeTitle = "ANY-SCP";
		private string studyInstanceUID = "";
		private string seriesInstanceUID = "";
		private string sopInstanceUID = "";
		private string outputDirectory = ".";
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

		// Study Instance UID to retrieve
		[Parameter(
			Mandatory = true,
			Position = 5,
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
			Position = 5,
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
			Position = 5,
			HelpMessage = "The SOP Instance UID to retrieve",
			ParameterSetName = "Image"
		)]
		public string SOPInstanceUID {
			get { return this.sopInstanceUID; }
			set { this.sopInstanceUID = value; }
		}

		// Output directory to save retrieved files
		[Parameter(
			Mandatory = false,
			Position = 6,
			HelpMessage = "The output directory to save retrieved DICOM files"
		)]
		[Alias("OutputPath")]
		public string OutputDirectory {
			get { return this.outputDirectory; }
			set { this.outputDirectory = value; }
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
		/// Process all C-GET requests
		/// </summary>
		protected override void ProcessRecord() {

			// resolve the output directory path
			string resolvedOutputDirectory;
			try {
				var resolvedPaths = SessionState.Path.GetResolvedPSPathFromPSPath(outputDirectory);
				resolvedOutputDirectory = resolvedPaths[0].Path;
			}
			catch {
				// if path doesn't exist yet, use GetUnresolvedProviderPathFromPSPath
				resolvedOutputDirectory = SessionState.Path.GetUnresolvedProviderPathFromPSPath(outputDirectory);
			}

			// create the output directory if it doesn't exist
			if (!Directory.Exists(resolvedOutputDirectory)) {
				try {
					Directory.CreateDirectory(resolvedOutputDirectory);
					WriteVerbose($"Created output directory: {resolvedOutputDirectory}");
				}
				catch (Exception e) {
					WriteWarning($"Unable to create output directory '{resolvedOutputDirectory}': {e.Message}");
					return;
				}
			}

			// write connection details if -Verbose switch supplied
			WriteVerbose($"Hostname:              {dicomRemoteHost}");
			WriteVerbose($"Port:                  {dicomRemoteHostPort}");
			WriteVerbose($"Calling AE Title:      {callingDicomAeTitle}");
			WriteVerbose($"Called AE Title:       {calledDicomAeTitle}");
			WriteVerbose($"Use TLS:               {useTls}");
			WriteVerbose($"Timeout:               {timeoutInSeconds}");
			WriteVerbose($"Output Directory:      {resolvedOutputDirectory}");

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

				// determine the query retrieve level and create the C-GET request
				DicomCGetRequest cGetRequest;
				if (sopInstanceUID.Length > 0) {
					cGetRequest = new DicomCGetRequest(studyInstanceUID, seriesInstanceUID, sopInstanceUID);
					WriteVerbose($"C-GET Level: IMAGE, SOP Instance UID: {sopInstanceUID}");
				}
				else if (seriesInstanceUID.Length > 0) {
					cGetRequest = new DicomCGetRequest(studyInstanceUID, seriesInstanceUID);
					WriteVerbose($"C-GET Level: SERIES, Series Instance UID: {seriesInstanceUID}");
				}
				else {
					cGetRequest = new DicomCGetRequest(studyInstanceUID);
					WriteVerbose($"C-GET Level: STUDY, Study Instance UID: {studyInstanceUID}");
				}

				// list to store the results returned
				var cGetResultList = new List<SendCGetResult>();
				int filesRetrieved = 0;
				int filesFailed = 0;
				string cGetStatus = "";

				// event handler - C-GET response received
				cGetRequest.OnResponseReceived += (request, response) => {
					verboseList.Add($"C-GET Response: {response.Status} (Remaining: {response.Remaining}, Completed: {response.Completed}, Failed: {response.Failures})");
					if (response.Status == DicomStatus.Success || response.Status == DicomStatus.Pending) {
						cGetStatus = response.Status.ToString();
						filesRetrieved = (int)response.Completed;
						filesFailed = (int)response.Failures;
					}
					else {
						cGetStatus = response.Status.ToString();
					}
				};

				// handler for incoming C-STORE sub-operations (server sends images back via C-STORE on the same association)
				client.OnCStoreRequest += (request) => {
					var sopClassUID = request.SOPClassUID.ToString();
					var instanceUID = request.SOPInstanceUID.UID;
					var fileName = $"{instanceUID}.dcm";
					var filePath = Path.Combine(resolvedOutputDirectory, fileName);

					try {
						request.File.Save(filePath);
						verboseList.Add($"Saved: {fileName}");
						cGetResultList.Add(new SendCGetResult(instanceUID, sopClassUID, filePath, "Success"));
					}
					catch (Exception e) {
						verboseList.Add($"Failed to save {fileName}: {e.Message}");
						cGetResultList.Add(new SendCGetResult(instanceUID, sopClassUID, filePath, $"Failed: {e.Message}"));
						return System.Threading.Tasks.Task.FromResult(new DicomCStoreResponse(request, DicomStatus.ProcessingFailure));
					}

					return System.Threading.Tasks.Task.FromResult(new DicomCStoreResponse(request, DicomStatus.Success));
				};

				// event handler - client association rejected by server
				client.AssociationRejected += (sender, eventArgs) => {
					verboseList.Add($"Association was rejected. Reason:{eventArgs.Reason}");
				};

				// event handler - client association accepted by server
				client.AssociationAccepted += (sender, eventArgs) => {
					verboseList.Add($"Association was accepted by:{eventArgs.Association.RemoteHost}");
				};

				// add the C-GET request to the client
				client.AddRequestAsync(cGetRequest);

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
					WriteWarning($"The C-GET request timed out (timeout set to {timeoutInSeconds} seconds). Use -Timeout to increase duration.");
				}
				else {
					// write the C-GET results to the pipeline
					if (cGetResultList.Count == 0) {
						WriteWarning($"No images were retrieved. Server status: {cGetStatus}");
					}
					else {
						WriteObject(cGetResultList);
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
