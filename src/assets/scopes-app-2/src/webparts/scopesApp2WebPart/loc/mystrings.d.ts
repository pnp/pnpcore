declare interface IScopesApp2Strings {
  PropertyPaneDescription: string;
  BasicGroupName: string;
  GraphScopeFieldLabel: string;
  SharePointScopeFieldLabel: string;
  AppLocalEnvironmentSharePoint: string;
  AppLocalEnvironmentTeams: string;
  AppLocalEnvironmentOffice: string;
  AppLocalEnvironmentOutlook: string;
  AppSharePointEnvironment: string;
  AppTeamsTabEnvironment: string;
  AppOfficeEnvironment: string;
  AppOutlookEnvironment: string;
  UnknownEnvironment: string;
}

declare module 'ScopesApp2Strings' {
  const strings: IScopesApp2Strings;
  export = strings;
}
