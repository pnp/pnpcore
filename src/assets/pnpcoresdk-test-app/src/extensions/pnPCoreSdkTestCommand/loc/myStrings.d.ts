declare interface IPnPCoreSdkTestCommandCommandSetStrings {
  Command1: string;
  Command2: string;
}

declare module 'PnPCoreSdkTestCommandCommandSetStrings' {
  const strings: IPnPCoreSdkTestCommandCommandSetStrings;
  export = strings;
}
