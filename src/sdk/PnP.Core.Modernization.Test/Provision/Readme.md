# Unit Testing Provisioning 

This area of unit testing is to provision a lightweight source site in SharePoint in which to transform from.

### Applies to ###

- SharePoint Online

## Provision Publishing Site

The publishing site uses an existing sample from PnP > Business Starter Kit from Franck Cornu @aequos 
This serves as a strong base to get started with a publishing portal, additonal page layouts can then be added to serve as scenarios in which to test out the transformation tool.

For installation please follow these instructions: [https://github.com/SharePoint/PnP/tree/master/Solutions/Business.StarterIntranet#set-up-your-environment](https://github.com/SharePoint/PnP/tree/master/Solutions/Business.StarterIntranet#set-up-your-environment)



## Provison Team Site

To provision a team site, we have prepared a sample set of pages using the PnP Provisioning engine.
Please create a new team site and then run the PnP PowerShell command:

```powershell

Connect-PnPOnline https://<your-tenant>.sharepoint.com/sites/<team-site>
Apply-PnProvisioningTemplate -Path "ClassicTeamSite-SampleData.xml"

```

## Assets

Logos and sample files are used from the PnP-Starter-Kit project, if you need more assets then, these projects can help:

-[https://github.com/SharePoint/sp-dev-provisioning-templates](https://github.com/SharePoint/sp-dev-provisioning-templates)
-[https://github.com/SharePoint/sp-starter-kit](https://github.com/SharePoint/sp-starter-kit)
