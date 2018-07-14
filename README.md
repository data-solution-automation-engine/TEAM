# TEAM
Taxonomy for ETL Automation Metadata. A collaboration project to work on the managing (creating, updating, deleting) and validating ETL generation metadata.

The TEAM application ties in to the Data Vault tooling ecosystem (see overview here: http://roelantvos.com/blog/?page_id=1919)

## Notes on obfuscation of libraries

The yWorks agreement requires us to obfuscate the yFiles libraries. To this extent I've used Dotfuscator Community Edition for Visual Studio 2017. The project / mapping files have been added to the TEAM solution. This will not trigger when running in the debug configuration (default), but only for the 'release' configuration the following command line post-build trigger has been added:

if $(ConfigurationName) == Release "D:\Microsoft Visual Studio 2017 Enterprise\Common7\IDE\Extensions\PreEmptiveSolutions\DotfuscatorCE\DotfuscatorCLI.exe" "D:\Git_Repositories\TEAM\TEAM\Dotfuscator\DotfuscatorTEAM.xml"

This will call the Dotfuscator CLI and prepare the yWorks libraries for packaging in the release / setup.

## Notes regarding use, specifically around the yFiles graph librarires
* yWorks has been generous enough to grant us a free full and perpetual yFiles .Net (WinForms / WPF) project license for the TEAM initiative. This TEAM-specific license has one floating seat for developers that are actively working on the yFiles part of TEAM. In addition it grants the team of collaborators - and build servers - the right to build the application. 
* The library comes with a watermark 'powered by yFiles'.
* The license is used under the condition / agreement that it is not part of the TEAM application as a sold product. TEAM is intended to be (limited) open source. If at some point the project is used to generate revenue this needs to be discussed with yWorks to change the development license to a regular one.
* As part of the agreement the yFiles libraries must be used in a private repository, accessible by a small team of ~20 collaborators 
* The compiled TEAM application including the yFiles libraries must be made publicly available only in binary, obfuscated or strongly signed form. See here: https://docs.yworks.com/yfilesdotnet/#/dguide/deployment.
* Usage of yFiles in TEAM will be announced in the documentation, About Dialogs and/or Credits appropriately with a link to the yWorks website: www.yworks.com.


