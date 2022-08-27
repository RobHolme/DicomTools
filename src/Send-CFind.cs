/* Filename:    SendCFind.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Date:        16/08/2021
 * 
 * Notes:       Implements a powershell CmdLet to send a DICOM C-FIND to a remote host and display results returned.
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

	[Cmdlet(VerbsCommunications.Send, "CFind")]
	public class SendCFind : PSCmdlet {

		private string dicomRemoteHost;
		private int dicomRemoteHostPort;
		private string callingDicomAeTitle = "DICOMTOOLS-SCU";
		private string calledDicomAeTitle = "ANY-SCP";
		private string patientName = "";
		private string patientID = "";
		private string studyID = "";
		private string accessionNumber = "";
		private string modalityType = "";
		private string studyDateStartString = "";
		private string studyDateEndString = "";
		private string studyRangeStartDate = "";
		private string studyRangeStartTime = "";
		private string studyRangeEndDate = "";
		private string studyRangeEndTime = "";
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

		// Patient name
		[Parameter(
			Mandatory = true,
			Position = 5,
			HelpMessage = "The patient name to search for",
			ParameterSetName = "PatientDemographics"
		)]
		public string PatientName {
			get { return this.patientName; }
			set { this.patientName = value; }
		}

		// Patient ID
		[Parameter(
			Mandatory = true,
			Position = 5,
			HelpMessage = "The patient name to search for",
			ParameterSetName = "PatientID"
		)]
		public string PatientID {
			get { return this.patientID; }
			set { this.patientID = value; }
		}

		// Study ID
		[Parameter(
			Mandatory = true,
			Position = 5,
			HelpMessage = "The Study ID to search for",
			ParameterSetName = "StudyID"
		)]
		public string StudyID {
			get { return this.studyID; }
			set { this.studyID = value; }
		}

		// Accession Number
		[Parameter(
			Mandatory = true,
			Position = 5,
			HelpMessage = "The Accession Number to search for",
			ParameterSetName = "AccessionNumber"
		)]
		public string AccessionNumber {
			get { return this.accessionNumber; }
			set { this.accessionNumber = value; }
		}

		// Constrain results to specific modalities
		[Parameter(
			Mandatory = false,
			Position = 6,
			HelpMessage = "Constrain results to specific modalities"
		)]
		public string Modality {
			get { return this.modalityType; }
			set { this.modalityType = value; }
		}

		// Search for studies acquired on or after this date
		[Parameter(
			Mandatory = false,
			Position = 7,
			HelpMessage = "Include studies from or after this date YYYYMMDD"
		)]
		public string StartDate {
			get { return this.studyDateStartString; }
			set { this.studyDateStartString = value; }
		}

		// Search for studies acquired on or before this date
		[Parameter(
			Mandatory = false,
			Position = 8,
			HelpMessage = "Include studies from or after this date YYYYMMDD"
		)]
		public string EndDate {
			get { return this.studyDateEndString; }
			set { this.studyDateEndString = value; }
		}

		// Use TLS for the connection
		[Parameter(
			Mandatory = false,
			Position = 9,
			HelpMessage = "Use TLS to secure the connection"
		)]
		public SwitchParameter UseTLS {
			get { return this.useTls; }
			set { this.useTls = value; }
		}

		// timeout waiting for a response from the server.
		[Parameter(
			Mandatory = false,
			Position = 10,
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
			if (studyDateStartString.Length > 0) {
				try {
					DateTime studyDateTimeStart = DateTime.Parse(studyDateStartString);
					studyRangeStartDate = $"{studyDateTimeStart.Year}{studyDateTimeStart.Month.ToString().PadLeft(2, '0')}{studyDateTimeStart.Day.ToString().PadLeft(2, '0')}";
					studyRangeStartTime = $"{studyDateTimeStart.Hour.ToString().PadLeft(2, '0')}{studyDateTimeStart.Minute.ToString().PadLeft(2, '0')}{studyDateTimeStart.Second.ToString().PadLeft(2, '0')}";
				}
				catch {
					WriteWarning($"Unable to convert '{studyDateStartString}' into a DateTime value for -StartDateTime.");
					abortProcessing = true;
					return;
				}
			}
			if (studyDateEndString.Length > 0) {
				try {
					DateTime studyDateTimeEnd = DateTime.Parse(studyDateEndString);
					studyRangeEndDate = $"{studyDateTimeEnd.Year}{studyDateTimeEnd.Month.ToString().PadLeft(2, '0')}{studyDateTimeEnd.Day.ToString().PadLeft(2, '0')}";
					studyRangeEndTime = $"{studyDateTimeEnd.Hour.ToString().PadLeft(2, '0')}{studyDateTimeEnd.Minute.ToString().PadLeft(2, '0')}{studyDateTimeEnd.Second.ToString().PadLeft(2, '0')}";
				}
				catch {
					WriteWarning($"Unable to convert '{studyDateEndString}' into a DateTime value for -EndDateTime.");
					abortProcessing = true;
					return;
				}
			}
		}


		/// <summary>
		/// Process all C-FIND requests
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
			WriteVerbose($"Study Start Date: {studyRangeStartDate}");
			WriteVerbose($"Study End Date:   {studyRangeEndDate}");

			var verboseList = new List<string>();

			try {
				// cancel token to cancel the request after a timeout
				CancellationTokenSource sourceCancelToken = new CancellationTokenSource();
				CancellationToken cancelToken = sourceCancelToken.Token;

				// create new DICOM client. Set timeout option based on -Timeout parameter use provides (defaults to 5 secs)
				var client = CreateClient(dicomRemoteHost, dicomRemoteHostPort, useTls, callingDicomAeTitle, calledDicomAeTitle, timeoutInSeconds);
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
				cFindRequest.Dataset.AddOrUpdate(DicomTag.AccessionNumber, accessionNumber);
				cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyDescription, "");
				cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyTime, "");
				if ((studyDateStartString.Length > 0) | (studyDateEndString.Length > 0)) {
					cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyDate, $"{studyRangeStartDate}-{studyRangeEndDate}");
				// Not all PACS implement the time range consistently. 
				// Some apply the time in addition to date range, i.e. only between the time range on each day, rather than start datetime and end datetime. 	
				// removing to avoid different experiences between PACS.
				//	cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyTime, $"{studyRangeStartTime}-{studyRangeEndTime}");
				}
				else {
					cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyDate, "");
				//	cFindRequest.Dataset.AddOrUpdate(DicomTag.StudyTime, "");
				}

				// The encoding of the results ('ISO_IR 100' is 'Latin Alphabet No. 1').  
				// http://dicom.nema.org/dicom/2013/output/chtml/part02/sect_D.6.html
				cFindRequest.Dataset.AddOrUpdate(new DicomTag(0x8, 0x5), "ISO_IR 100");

				// list to store the results returned
				var cFindResultList = new List<SendCFindResult>();

				// event handler - response received from C-Find request
				cFindRequest.OnResponseReceived += (request, response) => {
					if (response.Status == DicomStatus.Pending) {
						var responseModality = "";
						var responsePatientName = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);
						var responsePatientID = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
						var responsePatientDOB = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, string.Empty);
						var responsePatientSex = response.Dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty);
						string[] responseModalitiesInStudy = response.Dataset.GetValues<string>(DicomTag.ModalitiesInStudy);
						var responseStudyDate = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyDate, string.Empty);
						var responseStudyTime = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyTime, string.Empty);
						var responseStudyUID = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty);
						var responseAccessionNumber = response.Dataset.GetSingleValueOrDefault(DicomTag.AccessionNumber, string.Empty);
						var responseStudyDescription = response.Dataset.GetSingleValueOrDefault(DicomTag.StudyDescription, string.Empty);
						foreach (string modality in responseModalitiesInStudy) {
							responseModality += $",{modality}";
						}
						responseModality = responseModality.Substring(1);
						// convert the study date time string to a DateTime object (strip split seconds, too many different levels of precesion to handle)
						DateTime? responseStudyDateTime = ConvertDtToDateTime($"{responseStudyDate}{responseStudyTime.Split('.')[0]}");
						cFindResultList.Add(new SendCFindResult(responsePatientName, responsePatientID, responsePatientDOB, responsePatientSex, responseModality, responseStudyDateTime, responseStudyUID, responseAccessionNumber, responseStudyDescription));
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
					WriteWarning($"The C-FIND query timed out (timeout set to {timeoutInSeconds} seconds). Use -Timeout to increase duration.");
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