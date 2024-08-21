import * as React from 'react';
import * as ReactDom from 'react-dom';
import { Version } from '@microsoft/sp-core-library';
import {
  type IPropertyPaneConfiguration,
  PropertyPaneTextField
} from '@microsoft/sp-property-pane';
import { BaseClientSideWebPart } from '@microsoft/sp-webpart-base';
import { IReadonlyTheme } from '@microsoft/sp-component-base';

import * as strings from 'ScopesApp2Strings';
import ScopesApp2WebPart from './components/ScopesApp2WebPart';
import ErrorComponent from './components/ErrorComponent';

import { IScopesApp2WebPartProps } from './components/IScopesApp2WebPartProps';
import { AadTokenProvider } from '@microsoft/sp-http';

export interface IScopesApp2Props {
  graphResource: string;
  sharePointResource: string;
}

export default class ScopesApp2 extends BaseClientSideWebPart<IScopesApp2Props> {

  private _isDarkTheme: boolean = false;
  private _environmentMessage: string = '';

  async getTokens(tokenProvider: AadTokenProvider) : Promise<{graphToken: string, sharePointToken: string}>{
    const graphTokenTask = tokenProvider.getToken(this.properties.graphResource);
    const sharePointTokenTask = tokenProvider.getToken(this.properties.sharePointResource);

    return {
      graphToken: await graphTokenTask,
      sharePointToken: await sharePointTokenTask
    }
  }

  public async render(): Promise<void> {
    const tokenProvider = await this.context.aadTokenProviderFactory.getTokenProvider();
    try {
      const tokens = await this.getTokens(tokenProvider);
  
      const element: React.ReactElement<IScopesApp2WebPartProps> = React.createElement(
        ScopesApp2WebPart,
        {
          graphResource: this.properties.graphResource,
          sharePointResource: this.properties.sharePointResource,
          isDarkTheme: this._isDarkTheme,
          environmentMessage: this._environmentMessage,
          hasTeamsContext: !!this.context.sdks.microsoftTeams,
          userDisplayName: this.context.pageContext.user.displayName,
          graphToken: tokens.graphToken,
          sharePointToken: tokens.sharePointToken
        }
      );
      ReactDom.render(element, this.domElement);
    } catch (error) {
      ReactDom.render(
        React.createElement(ErrorComponent, {error}), this.domElement);
    }
  }

  protected onInit(): Promise<void> {
    return this._getEnvironmentMessage().then(message => {
      this._environmentMessage = message;
    });
  }



  private _getEnvironmentMessage(): Promise<string> {
    if (!!this.context.sdks.microsoftTeams) { // running in Teams, office.com or Outlook
      return this.context.sdks.microsoftTeams.teamsJs.app.getContext()
        .then(context => {
          let environmentMessage: string = '';
          switch (context.app.host.name) {
            case 'Office': // running in Office
              environmentMessage = this.context.isServedFromLocalhost ? strings.AppLocalEnvironmentOffice : strings.AppOfficeEnvironment;
              break;
            case 'Outlook': // running in Outlook
              environmentMessage = this.context.isServedFromLocalhost ? strings.AppLocalEnvironmentOutlook : strings.AppOutlookEnvironment;
              break;
            case 'Teams': // running in Teams
            case 'TeamsModern':
              environmentMessage = this.context.isServedFromLocalhost ? strings.AppLocalEnvironmentTeams : strings.AppTeamsTabEnvironment;
              break;
            default:
              environmentMessage = strings.UnknownEnvironment;
          }

          return environmentMessage;
        });
    }

    return Promise.resolve(this.context.isServedFromLocalhost ? strings.AppLocalEnvironmentSharePoint : strings.AppSharePointEnvironment);
  }

  protected onThemeChanged(currentTheme: IReadonlyTheme | undefined): void {
    if (!currentTheme) {
      return;
    }

    this._isDarkTheme = !!currentTheme.isInverted;
    const {
      semanticColors
    } = currentTheme;

    if (semanticColors) {
      this.domElement.style.setProperty('--bodyText', semanticColors.bodyText || null);
      this.domElement.style.setProperty('--link', semanticColors.link || null);
      this.domElement.style.setProperty('--linkHovered', semanticColors.linkHovered || null);
    }

  }

  protected onDispose(): void {
    ReactDom.unmountComponentAtNode(this.domElement);
  }

  protected get dataVersion(): Version {
    return Version.parse('1.0');
  }

  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return {
      pages: [
        {
          header: {
            description: strings.PropertyPaneDescription
          },
          groups: [
            {
              groupName: strings.BasicGroupName,
              groupFields: [
                PropertyPaneTextField('graphResource', {
                  label: strings.GraphScopeFieldLabel
                }),
                PropertyPaneTextField('sharePointResource', {
                  label: strings.SharePointScopeFieldLabel
                })
              ]
            }
          ]
        }
      ]
    };
  }
}
