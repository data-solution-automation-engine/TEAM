# TEAM

The Taxonomy for ETL Automation Metadata (TEAM) is a tool for design metadata management, geared towards data warehouse automation use-cases. It is part of the 'engine' for data warehouse automation, alongside the Virtual Data Warehouse pattern manager and the generic schema for Data Warehouse Automation.

* Additional information about the 'engine': https://roelantvos.com/blog/the-engine/
* Links to more information on the data warehouse automation ecosystem: https://roelantvos.com/blog/collaboration/
* An overview of the underpinning ideas on the metadata exchange format: https://roelantvos.com/blog/interface-for-data-warehouse-automation/
* The TEAM blog: https://roelantvos.com/blog/team/

## Way of working

Please report any bugs or issues as 'issues' in this Github. These issues will then be grouped into projects, for which I am creating a separate branch for each release.

Once tested, the branch is merged and a new development branch is started for the next planned release.

## Getting started

TEAM is packaged with a set of sample mappings, both in date warehouse automation JSON compliant format as well as tabular JSON.

For a really quick way to review the files and conventions, simply install the application, navigate to the Configuration menu and select 'Deploy Examples'. From here, click the 'Generate Sample Mapping Metadata' button.

This will copy the example file to the metadata directory, and code can be viewed and generated (in VDW) straight away. Open the Design Metadata grid to view the results, for example by right-clicking on a full row and select 'display'. Direct VDW to read these samples for further code generation.

## Troubleshooting

* TEAM will create directories if required, and when not existing yet. To do so, please install TEAM in a location that is less protected by the OS. For example, Program Files directories in Windows will usually require higher level of authorisation for applications to create directories (administrative priviliges). The app will report errors if directories and files cannot be created
* In some cases, configuration files from older versions will report a warning when used in a newer version. This warning will appear each time the app is started. The resolution is to save the configuration again, as this will upgrade the file to the latest format. This applies to JSON output handling and validation rules as well
