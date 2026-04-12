/// <summary>
/// An object containing the results to be returned to the pipeline for Send-CGet cmdlet.
/// </summary>

namespace DicomTools {

	public class SendCGetResult {
		private string sopInstanceUID;
		private string sopClassUID;
		private string filePath;
		private string status;

		/// <summary>
		/// The SOP Instance UID of the retrieved image
		/// </summary>
		public string SOPInstanceUID {
			get { return this.sopInstanceUID; }
			set { this.sopInstanceUID = value; }
		}

		/// <summary>
		/// The SOP Class UID of the retrieved image
		/// </summary>
		public string SOPClassUID {
			get { return this.sopClassUID; }
			set { this.sopClassUID = value; }
		}

		/// <summary>
		/// The file path where the image was saved
		/// </summary>
		public string FilePath {
			get { return this.filePath; }
			set { this.filePath = value; }
		}

		/// <summary>
		/// The status of the retrieval
		/// </summary>
		public string Status {
			get { return this.status; }
			set { this.status = value; }
		}

		/// <summary>
		/// Populate the class members with results from the C-GET
		/// </summary>
		public SendCGetResult(string SOPInstanceUID, string SOPClassUID, string FilePath, string Status) {
			this.sopInstanceUID = SOPInstanceUID;
			this.sopClassUID = SOPClassUID;
			this.filePath = FilePath;
			this.status = Status;
		}
	}
}
