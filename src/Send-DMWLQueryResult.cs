/// <summary>
/// An object containing the results to be returned to the pipeline for Send-DMWLQuery cmdlet.
/// </summary>

namespace DicomTools {
	using System;

	public class SendDMWLQueryResult {
		private string patientName;
		private string patientID;
		private string patientBirthDate;
		private string patientSex;
		private string modalitiesInStudy;
		private string modality;
		private DateTime? studyDate;
		private string studyInstanceID;
		private string accessionNumber;
		private string studyDescription;
		private string scheduledAETitle;
		private string stepCount;


		/// <summary>
		/// The patient name
		/// </summary>
		public string PatientName {
			get { return this.patientName; }
			set { this.patientName = value; }
		}

		/// <summary>
		/// The patient ID
		/// </summary>
		public string PatientID {
			get { return this.patientID; }
			set { this.patientID = value; }
		}

		/// <summary>
		/// The patient's date of birth
		/// </summary>
		public string BirthDate {
			get { return this.patientBirthDate; }
			set { this.patientBirthDate = value; }
		}

		/// <summary>
		/// The patient's sex
		/// </summary>
		public string Sex {
			get { return patientSex; }
			set { this.patientSex = value; }
		}

		/// <summary>
		/// The number of procedure steps
		/// </summary>
		public string Steps {
			get { return stepCount; }
			set { this.stepCount = value; }
		}

		/// <summary>
		/// The modality type
		/// </summary>
		public string Modality {
			get { return modality; }
			set { this.modality = value; }
		}

		/// <summary>
		/// The date the study was acquired
		/// </summary>
		public DateTime? StudyDate {
			get { return studyDate; }
			set { this.studyDate = value; }
		}

		/// <summary>
		/// The study AccessionNumber
		/// </summary>
		public string AccessionNumber {
			get { return accessionNumber; }
			set { this.accessionNumber = value; }
		}

		/// <summary>
		/// The study Description
		/// </summary>
		public string StudyDescription {
			get { return studyDescription; }
			set { this.studyDescription = value; }
		}


		/// <summary>
		/// Populate the class members with resuls from the C-Echo
		/// </summary>
		/// <param name="ItemValue"></param>
//		public SendCFindResult(string PatientName, string PatientID, string PatientBirthDate, string PatientSex, string ModalitiesInStudy, DateTime? StudyDate, string StudyInstanceID, string AccessionNumber, string StudyDescription) {
			public SendDMWLQueryResult(string PatientName, string PatientID, string PatientBirthDate, string PatientSex, string StepCount, string Modality, string AccessionNumber, string StudyDescription) {
			this.patientName = PatientName;
			this.patientID = PatientID;
			this.patientBirthDate = PatientBirthDate;
			this.patientSex = PatientSex;
			this.stepCount = StepCount;
			this.modality = Modality;
			this.studyDescription = StudyDescription;
			this.accessionNumber = AccessionNumber;
			this.studyDate = StudyDate;
//			this.studyInstanceID = StudyInstanceID;
//			this.accessionNumber = AccessionNumber;
//			this.studyDescription = StudyDescription;
		}
	}
}

