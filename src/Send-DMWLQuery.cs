/* Filename:    SendDMWLQuery.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Date:        21/11/2021
 * 
 * Notes:       Implements a powershell CmdLet to send a DICOM Modality Worklist (DMWL) Query.
 * 
 */


namespace DicomTools {
	using System;
	using System.Collections.Generic;
	using System.Management.Automation;
	using System.Text.RegularExpressions;
	using System.Threading;
	using Dicom;
	using Dicom.Network;

	[Cmdlet(VerbsCommunications.Send, "DMWLQuery")]
	public class SendDMWLQuery : PSCmdlet {

		private string dicomRemoteHost;
		private int dicomRemoteHostPort;
		private string callingDicomAeTitle = "DICOMTOOLS-SCU";
		private string calledDicomAeTitle = "ANY-SCP";
		private string stationAETitle = null;
		private string stationName = null;
		private string patientName = null;
		private string patientID = null;
		private string modalityType = null;
		private string scheduledDateTime = "";
		private string scheduledStartDate = null;
		private string scheduledStartTime = null;
		private bool useTls = false;
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

		// The Station AE title
		[Parameter(
			Mandatory = false,
			Position = 5,
			HelpMessage = "The station AE title"
		)]

		// Constrain results to specific modalities
		[Parameter(
			Mandatory = false,
			Position = 8,
			HelpMessage = "Constrain results to specific modalities"
		)]
		public string Modality {
			get { return this.modalityType; }
			set { this.modalityType = value; }
		}

		// Search for studies acquired on or after this date
		[Parameter(
			Mandatory = false,
			Position = 9,
			HelpMessage = "Include studies scheduled for this date/time"
		)]
		public string ScheduledDateTime {
			get { return this.scheduledDateTime; }
			set { this.scheduledDateTime = value; }
		}

		// Use TLS for the connection
		[Parameter(
			Mandatory = false,
			Position = 10,
			HelpMessage = "Use TLS to secure the connection"
		)]
		public SwitchParameter UseTLS {
			get { return this.useTls; }
			set { this.useTls = value; }
		}

		// timeout waiting for a response from the server.
		[Parameter(
			Mandatory = false,
			Position = 11,
			HelpMessage = "The timeout in seconds to wait for a response"
		)]
		[ValidateRange(1, 60)]
		public int Timeout {
			get { return this.timeoutInSeconds; }
			set { this.timeoutInSeconds = value; }
		}

		/// <summary>
		/// begin processing
		/// </summary>
		protected override void BeginProcessing() {


			// Convert the user date time values into a DateTime object. Use standard parser, warn if unable to parse. 
			// Conversion is done here, instead of using a [DateTime] parameter to allow friendlier error reporting if value can;t be parsed as a DateTime value.
			if (scheduledDateTime.Length > 0) {
				try {
					DateTime studyScheduledDateTimeStart = DateTime.Parse(scheduledDateTime);
					scheduledStartDate = $"{studyScheduledDateTimeStart.Year}{studyScheduledDateTimeStart.Month.ToString().PadLeft(2, '0')}{studyScheduledDateTimeStart.Day.ToString().PadLeft(2, '0')}";
					scheduledStartTime = $"{studyScheduledDateTimeStart.Hour.ToString().PadLeft(2, '0')}{studyScheduledDateTimeStart.Minute.ToString().PadLeft(2, '0')}{studyScheduledDateTimeStart.Second.ToString().PadLeft(2, '0')}";
				}
				catch {
					WriteWarning($"Unable to convert '{scheduledDateTime}' into a DateTime value for -StartDateTime.");
					abortProcessing = true;
					return;
				}
			}
		}


		/// <summary>
		/// Process all DMWL requests
		/// </summary>
		protected override void ProcessRecord() {
			if (abortProcessing) {
				return;
			}

			// write connection details if -Verbose switch supplied
			WriteVerbose($"Hostname:              {dicomRemoteHost}");
			WriteVerbose($"Port:                  {dicomRemoteHostPort}");
			WriteVerbose($"Calling AE Title:      {callingDicomAeTitle}");
			WriteVerbose($"Called AE Title:       {calledDicomAeTitle}");
			WriteVerbose($"Use TLS:               {useTls}");
			WriteVerbose($"Timeout:               {timeoutInSeconds}");

			var verboseList = new List<string>();

			try {
				// cancel token to cancel the request after a timeout
				CancellationTokenSource sourceCancelToken = new CancellationTokenSource();
				CancellationToken cancelToken = sourceCancelToken.Token;

				// create new DICOM client. Set timeout option based on -Timeout parameter use provides (defaults to 5 secs)
				var client = new Dicom.Network.Client.DicomClient(dicomRemoteHost, dicomRemoteHostPort, useTls, callingDicomAeTitle, calledDicomAeTitle);
				client.Options = new Dicom.Network.DicomServiceOptions();
				client.Options.RequestTimeout = new TimeSpan(0, 0, timeoutInSeconds);
				client.NegotiateAsyncOps();

				var cFindRequest = DicomCFindRequest.CreateWorklistQuery(); // no filter, return all results.

				// list to store the results returned
				var cFindResultList = new List<SendDMWLQueryResult>();

				// event handler - response received from C-Find request
				cFindRequest.OnResponseReceived += (request, response) => {
					if (response.Status == DicomStatus.Pending) {
						var responsePatientName = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);
						var responsePatientID = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
						var responsePatientDOB = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, string.Empty);
						var responsePatientSex = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty);
						var responseAccessionNumber = response.Dataset.GetSingleValueOrDefault(DicomTag.AccessionNumber, string.Empty);
						var responseRequestedProcedureDescription = response.Dataset.GetSingleValueOrDefault(DicomTag.RequestedProcedureDescription, string.Empty);
						// get the DICOM step sequence
						DicomSequence stepSequence = response.Dataset.GetSequence(DicomTag.ScheduledProcedureStepSequence);
						var responseModality = "";
						var dmwlStepList = new List<DMWLStepResult>();
						foreach (var step in stepSequence.Items) {
							responseModality += $",{step.GetSingleValueOrDefault(DicomTag.Modality, string.Empty)}";
							DateTime? scheduledProcedureStudyDateTime = ConvertDtToDateTime($"{step.GetSingleValueOrDefault(DicomTag.ScheduledProcedureStepStartDate, string.Empty)}{step.GetSingleValueOrDefault(DicomTag.ScheduledProcedureStepStartTime, string.Empty).Split('.')[0]}");
							dmwlStepList.Add(new DMWLStepResult(
								step.GetSingleValueOrDefault(DicomTag.ScheduledProcedureStepID, string.Empty),
								step.GetSingleValueOrDefault(DicomTag.Modality, string.Empty),
								step.GetSingleValueOrDefault(DicomTag.ScheduledPerformingPhysicianName, string.Empty),
								scheduledProcedureStudyDateTime,
								step.GetSingleValueOrDefault(DicomTag.ScheduledProcedureStepDescription, string.Empty)
								)
							);
						}
						responseModality = responseModality.Substring(1);
						cFindResultList.Add(new SendDMWLQueryResult(responsePatientName, responsePatientID, responsePatientDOB, responsePatientSex, stepSequence.Items.Count.ToString(), responseModality, responseAccessionNumber, responseRequestedProcedureDescription, dmwlStepList));
					}
				};

				// event handler - client association rejected by server
				client.AssociationRejected += (sender, eventArgs) => {
					verboseList.Add($"Association was rejected. Reason:{eventArgs.Reason}");
				};

				// event handler - client association accepted by server
				client.AssociationAccepted += (sender, eventArgs) => {
					verboseList.Add($"Association was accepted by:{eventArgs.Association.RemoteHost}");
				};

				// add the C-FIND request to the client
				client.AddRequestAsync(cFindRequest);

				// send an async request, wait for response (Powershell output can't be from a thread). 
				// cancel after period specified by -Timeout parameter
				sourceCancelToken.CancelAfter(timeoutInSeconds * 1000);
				var task = client.SendAsync(cancelToken);
				task.Wait();

				// write verbose logging from the async event handlers (cant write to pwsh host from anther thread)
				verboseList.Reverse();
				foreach (string verboseString in verboseList) {
					WriteVerbose(verboseString);
				}

				// check to see if the task timed out, otherwise return results.
				if (cancelToken.IsCancellationRequested) {
					WriteWarning($"The DMWL query timed out (timeout set to {timeoutInSeconds} seconds). Use -Timeout to increase duration.");
				}
				else {
					// write the C-FIND results to the pipeline
					if (cFindResultList.Count == 0) {
						WriteWarning($"No matching records found.");
					}
					else {
						WriteObject(cFindResultList);
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

		/// <summary>
		/// Convert a DICOM DT string to a DateTime Object.
		//	If unable to detect format, return DateTime.MinValue
		/// </summary>
		private DateTime? ConvertDtToDateTime(string DicomDtString) {
			try {

				// Year Month Day Hour Minute Second SplitSecond (6 digits) Timezone
				if (Regex.IsMatch(DicomDtString, @"^[0-9]{14}\.[0-9]{6}\+[0-9]{4}$")) {
					return DateTime.ParseExact(DicomDtString, "yyyyMMddHHmmss.ffffffzzz", null);
				}

				// Year Month Day Hour Minute Second SplitSecond (3 digits) Timezone
				if (Regex.IsMatch(DicomDtString, @"^[0-9]{14}\.[0-9]{3}\+[0-9]{4}$")) {
					return DateTime.ParseExact(DicomDtString, "yyyyMMddHHmmss.fffzzz", null);
				}

				// Year Month Day Hour Minute Second SplitSecond (6 digits)
				if (Regex.IsMatch(DicomDtString, @"^[0-9]{14}\.[0-9]{6}")) {
					return DateTime.ParseExact(DicomDtString, "yyyyMMddHHmmss.ffffff", null);
				}

				// Year Month Day Hour Minute Second SplitSecond (3 digits)
				if (Regex.IsMatch(DicomDtString, @"^[0-9]{14}\.[0-9]{3}")) {
					return DateTime.ParseExact(DicomDtString, "yyyyMMddHHmmss.fff", null);
				}

				// Year Month Day Hour Minute Second 
				if (Regex.IsMatch(DicomDtString, @"^[0-9]{14}")) {
					return DateTime.ParseExact(DicomDtString, "yyyyMMddHHmmss", null);
				}

				// Year Month Day Hour Minute 
				if (Regex.IsMatch(DicomDtString, @"^[0-9]{12}")) {
					return DateTime.ParseExact(DicomDtString, "yyyyMMddHHmm", null);
				}

				// Year Month Day 
				if (Regex.IsMatch(DicomDtString, @"^[0-9]{8}")) {
					return DateTime.ParseExact(DicomDtString, "yyyyMMdd", null);
				}

				// not match found
				return null;
			}
			catch {
				return null;
			}
		}
	}
}