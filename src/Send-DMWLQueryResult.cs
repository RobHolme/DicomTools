/// <summary>
/// An object containing the results to be returned to the pipeline for Send-DMWLQuery cmdlet.
/// </summary>

namespace DicomTools {
	using System;
	using System.Collections.Generic;

	public class SendDMWLQueryResult {
		private string patientName;
		private string patientID;
		private string patientBirthDate;
		private string patientSex;
		private string modality;
		private string accessionNumber;
		private string studyDescription;
		private string stepCount;
		private List<DicomTools.DMWLStepResult> procedureSteps;


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
		/// The list of procedure steps
		/// </summary>
		public List<DicomTools.DMWLStepResult> ProcedureSteps {
			get { return procedureSteps; }
			set { this.procedureSteps = value; }
		}

		/// <summary>
		/// Populate the class members with resuls from the C-Echo
		/// </summary>
		/// <param name="ItemValue"></param>
			public SendDMWLQueryResult(string PatientName, string PatientID, string PatientBirthDate, string PatientSex, string StepCount, string Modality, string AccessionNumber, string StudyDescription, List<DicomTools.DMWLStepResult> ProcedureSteps) {
			this.patientName = PatientName;
			this.patientID = PatientID;
			this.patientBirthDate = PatientBirthDate;
			this.patientSex = PatientSex;
			this.stepCount = StepCount;
			this.modality = Modality;
			this.studyDescription = StudyDescription;
			this.accessionNumber = AccessionNumber;
			this.procedureSteps = ProcedureSteps;

		}
	}


	
	/// <summary>
	/// An object containing individual DMWL step details
	/// </summary>
	public class DMWLStepResult {
		private string stepId;
		private string modality;
		private string performingPhysician;
		private string stepDescription;
		private DateTime? stepDateTime;
		
		/// <summary>
		/// The step ID
		/// </summary>
		public string StepID {
			get { return this.stepId; }
			set { this.stepId = value; }
		}

		/// <summary>
		/// The modality type performing the step
		/// </summary>
		public string Modality {
			get { return this.modality; }
			set { this.modality = value; }
		}

		/// <summary>
		/// The name of the physician  performing the step
		/// </summary>
		public string PerformingPhysician {
			get { return this.performingPhysician; }
			set { this.performingPhysician = value; }
		}

		/// <summary>
		/// The date the study was acquired
		/// </summary>
		public DateTime? StepDateTime {
			get { return stepDateTime; }
			set { this.stepDateTime = value; }
		}

		/// <summary>
		/// The step description
		/// </summary>
		public string StepDescription {
			get { return this.stepDescription; }
			set { this.stepDescription = value; }
		}


		/// <summary>
		/// Populate the class members with resuls from the C-Echo
		/// </summary>
		/// <param name="ItemValue"></param>
			public DMWLStepResult(string StepID, string Modality, string PerformingPhysician, DateTime? StepDateTime, string StepDescription) {
			this.stepId = StepID;
			this.modality = Modality;
			this.performingPhysician = PerformingPhysician;
			this.modality = Modality;
			this.stepDescription = StepDescription;
			this.stepDateTime = StepDateTime;

		}

	}
}

