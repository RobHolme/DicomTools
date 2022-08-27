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
	using FellowOakDicom;
	using FellowOakDicom.Network;
	using FellowOakDicom.Network.Client;

	[Cmdlet(VerbsCommunications.Send, "DMWLQuery")]
	public class SendDMWLQuery : PSCmdlet {

		private string dicomRemoteHost;
		private int dicomRemoteHostPort;
		private string callingDicomAeTitle = "DICOMTOOLS-SCU";
		private string calledDicomAeTitle = "ANY-SCP";
		private string patientName = null;
		private string patientID = null;
		private string modalityType = null;
		private string studyScheduledStartString = null;
		private string studyScheduledEndString = null;
		private DicomDateRange scheduledDateTimeRange = null;
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

		// Search for scheduled exams for specific patients
		[Parameter(
			Mandatory = false,
			Position = 5,
			HelpMessage = "Include studies scheduled for this Patient name"
		)]
		public string PatientName {
			get { return this.patientName; }
			set { this.patientName = value; }
		}

		// Search for scheduled exams for specific patients
		[Parameter(
			Mandatory = false,
			Position = 6,
			HelpMessage = "Include studies scheduled for this Patient ID"
		)]
		public string PatientID {
			get { return this.patientID; }
			set { this.patientID = value; }
		}

		// Constrain results to specific modalities
		[Parameter(
			Mandatory = false,
			Position = 7,
			HelpMessage = "Constrain results to specific modalities"
		)]
		public string Modality {
			get { return this.modalityType; }
			set { this.modalityType = value; }
		}

		// Search for studies scheduled on or after this date /time
		[Parameter(
			Mandatory = false,
			Position = 8,
			HelpMessage = "Include studies scheduled from or after this date (and time)"
		)]
		public string StartDateTime {
			get { return this.studyScheduledStartString; }
			set { this.studyScheduledStartString = value; }
		}

		// Search for studies acquired on or before this date
		[Parameter(
			Mandatory = false,
			Position = 9,
			HelpMessage = "Include studies from or after this date YYYYMMDD"
		)]
		public string EndDateTime {
			get { return this.studyScheduledEndString; }
			set { this.studyScheduledEndString = value; }
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
			DateTime studyScheduledStartDateTime;
			DateTime studyScheduledEndDateTime;

			// the search is based on date range, so start and end times for the range must be included.  
			if ((studyScheduledStartString != null) & (studyScheduledEndString == null)) {
				WriteWarning($"The -EndDateTime parameter must be supplied with the -StartDateTime parameter");
				abortProcessing = true;
				return;
			}
			if ((studyScheduledEndString != null) & (studyScheduledStartString == null)) {
				WriteWarning($"The -StartDateTime parameter must be supplied with the -EndDateTime parameter");
				abortProcessing = true;
				return;
			}


			// Convert the user date time values into a DateTime object. Use standard parser, warn if unable to parse. 
			// Conversion is done here, instead of using a [DateTime] parameter to allow friendlier error reporting if value can;t be parsed as a DateTime value.
			if (studyScheduledStartString != null) {
				try {
					studyScheduledStartDateTime = DateTime.Parse(studyScheduledStartString);
				}
				catch {
					WriteWarning($"Unable to convert '{studyScheduledStartString}' into a DateTime value for -StartDateTime.");
					abortProcessing = true;
					return;
				}

				if (studyScheduledEndString != null) {
					try {
						studyScheduledEndDateTime = DateTime.Parse(studyScheduledEndString);
					}
					catch {
						WriteWarning($"Unable to convert '{studyScheduledEndString}' into a DateTime value for -StartDateTime.");
						abortProcessing = true;
						return;
					}

					// create a date range object of start and end datetime parameters supplied
					if ((studyScheduledStartDateTime != null) & (studyScheduledEndDateTime != null)) {
						scheduledDateTimeRange = new DicomDateRange(studyScheduledStartDateTime, studyScheduledEndDateTime);
					}
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
			WriteVerbose($"Hostname:          {this.dicomRemoteHost}");
			WriteVerbose($"Port:              {this.dicomRemoteHostPort}");
			WriteVerbose($"Calling AE Title:  {this.callingDicomAeTitle}");
			WriteVerbose($"Called AE Title:   {this.calledDicomAeTitle}");
			WriteVerbose($"Use TLS:           {this.useTls}");
			WriteVerbose($"Timeout:           {this.timeoutInSeconds}");
			WriteVerbose("");
			WriteVerbose("------Schedule Exam Search Parameters------");
			WriteVerbose($"Patient Name:             {this.patientName}");
			WriteVerbose($"Patient ID:               {this.patientID}");
			if (scheduledDateTimeRange != null) {
				WriteVerbose($"Scheduled Exam DateTime:  {this.scheduledDateTimeRange.ToString()}");
			}
			else {
				WriteVerbose($"Scheduled Exam DateTime:");
			}
			WriteVerbose($"Modality Type:            {this.modalityType}");
			WriteVerbose("");
			var verboseList = new List<string>();

			try {
				// cancel token to cancel the request after a timeout
				CancellationTokenSource sourceCancelToken = new CancellationTokenSource();
				CancellationToken cancelToken = sourceCancelToken.Token;

				// create new DICOM client. Set timeout option based on -Timeout parameter use provides (defaults to 5 secs)
				var client = CreateClient(dicomRemoteHost, dicomRemoteHostPort, useTls, callingDicomAeTitle, calledDicomAeTitle, timeoutInSeconds);
				client.NegotiateAsyncOps();

				// no filter, return all results.
				var cFindRequest = DicomCFindRequest.CreateWorklistQuery(
						patientName: this.PatientName,
						patientId: this.PatientID,
						modality: this.modalityType,
						scheduledDateTime: scheduledDateTimeRange
				);

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

		// create a new DICOM client, set default options
		private IDicomClient CreateClient(string host, int port, bool useTls, string callingAe, string calledAe, int timeout)
        {
            var client = DicomClientFactory.Create(host, port, useTls, callingAe, calledAe);
            client.ServiceOptions.LogDimseDatasets = false;
            client.ServiceOptions.LogDataPDUs = false;
			client.ServiceOptions.RequestTimeout = new TimeSpan(0, 0, timeout);
            return client;
        }

	}
}