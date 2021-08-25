/// <summary>
/// An object containing the results to be returned to the pipeline for Send-CFind cmdlet.
/// </summary>

namespace DicomTools {
	using System;

	public class SendCFindResult {
		private string patientName;
		private string patientID;
		private string patientBirthDate;
		private string patientSex;
		private string modalitiesInStudy;
		private DateTime studyDate;
		private string studyInstanceID;


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
		/// The modality type
		/// </summary>
		public string ModalitiesInStudy {
			get { return modalitiesInStudy; }
			set { this.modalitiesInStudy = value; }
		}

		/// <summary>
		/// The date the study was acquired
		/// </summary>
		public DateTime StudyDate {
			get { return studyDate; }
			set { this.studyDate = value; }
		}

		/// <summary>
		/// The study instance ID
		/// </summary>
		public string StudyInstanceID {
			get { return studyInstanceID; }
			set { this.studyInstanceID = value; }
		}

		/// <summary>
		/// Populate the class members with resuls from the C-Echo
		/// </summary>
		/// <param name="ItemValue"></param>
		public SendCFindResult(string PatientName, string PatientID, string PatientBirthDate, string PatientSex, string ModalitiesInStudy, DateTime StudyDate, string StudyInstanceID) {
			this.patientName = PatientName;
			this.patientID = PatientID;
			this.patientBirthDate = PatientBirthDate;
			this.patientSex = PatientSex;
			this.modalitiesInStudy = ModalitiesInStudy;
			this.studyDate = StudyDate;
			this.studyInstanceID = StudyInstanceID;
		}
	}
}

