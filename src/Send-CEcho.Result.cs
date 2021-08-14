/// <summary>
    /// An object containing the results to be returned to the pipeline for Send-CEcho commandlet.
    /// </summary>

namespace DicomTools {

    public class SendCEchoResult
    {
        private string hostname;
		private int port;
        private string status;
		private long responseTime;

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
        /// Populate the class members with resuls from the C-Echo
        /// </summary>
        /// <param name="ItemValue"></param>
        public SendCEchoResult(string Hostname, int Port, string Status, long ResponseTime)
        {
            this.hostname = Hostname;
			this.port = Port;
			this.status = Status;
			this.responseTime = ResponseTime;
        }
	}
}

    