/// <summary>
    /// An object containing the results to be returned to the pipeline for Send-CEcho cmdlet.
    /// </summary>

namespace DicomTools {

    public class SendCEchoResult
    {
        private string hostname;
		private int port;
        private string status;
		private double responseTime;

        /// <summary>
        /// The hostname of the DICOM interface
        /// </summary>
        public string Hostname
        {
            get { return this.hostname; }
            set { this.hostname = value; }
        }

        /// <summary>
        /// The port number of the DICOM interface
        /// </summary>
        public int Port
        {
            get { return this.port; }
            set { this.port = value; }
        }

		/// <summary>
        /// The status of the CEcho request
        /// </summary>
        public string Status
        {
            get { return this.status; }
            set { this.status = value; }
        }

		/// <summary>
        /// The time taken to complete the request
        /// </summary>
        public double ResponseTime
        {
            get { return responseTime; }
            set { this.responseTime = value; }
        }


        /// <summary>
        /// Populate the class members with resuls from the C-Echo
        /// </summary>
        /// <param name="ItemValue"></param>
        public SendCEchoResult(string Hostname, int Port, string Status, double ResponseTime)
        {
            this.hostname = Hostname;
			this.port = Port;
			this.status = Status;
			this.responseTime = ResponseTime;
        }
	}
}

    