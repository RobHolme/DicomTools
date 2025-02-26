# Change Log
* 0.1.0	- Added Send-CFind cmdlet.
* 0.2.0 - Added Get-DicomTag cmdlet.
* 1.0.0 - Added Send-CFind cmdlet.
* 1.1.0 - Added ability to search on Study ID in Send-CFind cmdlet
* 1.2.0 - Added -Modality switch to constrain search to specific modality types
        - Add -StartDate and -EndDate to constrain date range of search
* 1.2.1 - Added timeout for C-ECHO and C-FIND to receive a response (same timeout also applied waiting for an association to complete)
* 1.2.2 - Minor changes to verbose logging. Improved result reporting on timeout. 
* 1.2.3 - User warned if a timeout was reached.
* 1.2.4 - Added support for reporting all Modalities in a study.  
* 1.2.5 - Added -AccessionNumber parameter to search on Accession Number.
* 1.2.6 - Added help file
* 1.2.7 - Updated module to support Windows Powershell (in addition to Microsoft Powershell (core))
* 1.2.8 - Incremented module version to fix publishing error on PSGallery
* 1.2.9 - Added alias 'CallingAETitle' to LocalAETitle parameter. Added alias 'CalledAETitle' to RemoteAETitle parameter  (for both Send-CEcho and Send-CFind).
* 1.3.0 - Added Send-DMWLQuery function to query DICOM Modality Worklists
* 1.3.1 - Added additional search parameters for Send-DMWLQuery function
* 1.3.2 - Using .net 4.8 for .net Framework targets. 
        - Fixed issue in send-dmwlquery where date range is not supplied.
* 1.4.0 - Removed support for .Net Framework. Only .Net Core is now supported (PowerShell v6+)          
* 1.4.1 - Moved to .Net 6.0. No functional changes. 
* 1.4.2 - Updated fo-dicom library to 5.1.1