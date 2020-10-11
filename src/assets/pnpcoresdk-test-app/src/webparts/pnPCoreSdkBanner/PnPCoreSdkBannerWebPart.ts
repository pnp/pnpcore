import * as React from 'react';
import * as ReactDom from 'react-dom';
import { Version } from '@microsoft/sp-core-library';
import {
  IPropertyPaneConfiguration,
  PropertyPaneTextField
} from '@microsoft/sp-property-pane';
import { BaseClientSideWebPart } from '@microsoft/sp-webpart-base';

import * as strings from 'PnPCoreSdkBannerWebPartStrings';
import { Banner, IBannerProps } from '../../sharedComponents/banner/Banner';

export interface IPnPCoreSdkBannerWebPartProps {
  message: string;
}

export default class PnPCoreSdkBannerWebPart extends BaseClientSideWebPart<IPnPCoreSdkBannerWebPartProps> {

  public render(): void {
    const element: React.ReactElement<IBannerProps> = React.createElement(Banner, {
      message: this.properties.message
    });
    ReactDom.render(element, this.domElement);
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
                PropertyPaneTextField('IBannerProps', {
                  label: strings.MessagePropLabel
                })
              ]
            }
          ]
        }
      ]
    };
  }
}
