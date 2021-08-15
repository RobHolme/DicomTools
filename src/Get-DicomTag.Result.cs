	/// <summary>
    /// An object containing the results to be returned to the pipeline for Get-DicomTag cmdlet.
    /// </summary>

namespace DicomTools {

    public class GetDicomTagResult
    {
        private string dicomTag;
		private string value;
		private string filename;
		
        /// <summary>
        /// The DICOM tag name
        /// </summary>
        public string DicomTag
        {
            get { return this.dicomTag; }
            set { this.dicomTag = value; }
        }

        /// <summary>
        /// The value of the DicomTag
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

		/// <summary>
        /// The filename of the DICOM file
        /// </summary>
        public string Filename
        {
            get { return this.filename; }
            set { this.filename = value; }
        }

		
        /// <summary>
        /// Populate the class members with resuls from the C-Echo
        /// </summary>
        /// <param name="ItemValue"></param>
        public GetDicomTagResult(string DicomTag, string Value, string Filename)
        {
            this.dicomTag = DicomTag;
			this.value = Value;
			this.filename = Filename;
        }
	}
}

    