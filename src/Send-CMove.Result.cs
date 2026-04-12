/// <summary>
/// An object containing the results to be returned to the pipeline for Send-CMove cmdlet.
/// </summary>

namespace DicomTools {

	public class SendCMoveResult {
		private string status;
		private int completed;
		private int failed;
		private int remaining;
		private int warning;
		private string moveDestination;

		/// <summary>
		/// The status of the C-MOVE operation
		/// </summary>
		public string Status {
			get { return this.status; }
			set { this.status = value; }
		}

		/// <summary>
		/// The number of completed sub-operations
		/// </summary>
		public int Completed {
			get { return this.completed; }
			set { this.completed = value; }
		}

		/// <summary>
		/// The number of failed sub-operations
		/// </summary>
		public int Failed {
			get { return this.failed; }
			set { this.failed = value; }
		}

		/// <summary>
		/// The number of remaining sub-operations
		/// </summary>
		public int Remaining {
			get { return this.remaining; }
			set { this.remaining = value; }
		}

		/// <summary>
		/// The number of warning sub-operations
		/// </summary>
		public int Warning {
			get { return this.warning; }
			set { this.warning = value; }
		}

		/// <summary>
		/// The AE title of the move destination
		/// </summary>
		public string MoveDestination {
			get { return this.moveDestination; }
			set { this.moveDestination = value; }
		}

		/// <summary>
		/// Populate the class members with results from the C-MOVE
		/// </summary>
		public SendCMoveResult(string Status, int Completed, int Failed, int Remaining, int Warning, string MoveDestination) {
			this.status = Status;
			this.completed = Completed;
			this.failed = Failed;
			this.remaining = Remaining;
			this.warning = Warning;
			this.moveDestination = MoveDestination;
		}
	}
}
