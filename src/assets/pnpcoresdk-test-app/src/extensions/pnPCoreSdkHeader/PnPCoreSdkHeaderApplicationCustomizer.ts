import * as React from 'react';
import * as ReactDom from 'react-dom';
import { override } from '@microsoft/decorators';
import {
  BaseApplicationCustomizer, PlaceholderContent, PlaceholderName
} from '@microsoft/sp-application-base';

const LOG_SOURCE: string = 'PnPCoreSdkHeaderApplicationCustomizer';

import { Banner, IBannerProps } from '../../sharedComponents/banner/Banner';

/**
 * If your command set uses the ClientSideComponentProperties JSON input,
 * it will be deserialized into the BaseExtension.properties object.
 * You can define an interface to describe it.
 */
export interface IPnPCoreSdkHeaderApplicationCustomizerProperties {
  message: string;
}

/** A Custom Action which can be run during execution of a Client Side Application */
export default class PnPCoreSdkHeaderApplicationCustomizer
  extends BaseApplicationCustomizer<IPnPCoreSdkHeaderApplicationCustomizerProperties> {

  private _topPlaceholder: PlaceholderContent;

  @override
  public onInit(): Promise<void> {
    this.context.placeholderProvider.changedEvent.add(this, this._renderPlaceHolders);

    return Promise.resolve();
  }

  private _renderPlaceHolders(): void {
    if (!this._topPlaceholder) {
      this._topPlaceholder =
        this.context.placeholderProvider.tryCreateContent(PlaceholderName.Top);
    }

    const element: React.ReactElement<IBannerProps> = React.createElement(Banner, {
      message: this.properties.message
    });
    ReactDom.render(element, this._topPlaceholder.domElement);
    
  }
}
