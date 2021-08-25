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