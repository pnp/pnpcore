import * as React from 'react';
import styles from './ScopesApp2WebPart.module.scss';
import type { IScopesApp2WebPartProps } from './IScopesApp2WebPartProps';
import { escape } from '@microsoft/sp-lodash-subset';
import { jwtDecode } from "jwt-decode";

export default class ScopesApp2WebPart extends React.Component<IScopesApp2WebPartProps, {}> {
  public render(): React.ReactElement<IScopesApp2WebPartProps> {
    const {
      graphResource,
      sharePointResource,
      isDarkTheme,
      environmentMessage,
      hasTeamsContext,
      userDisplayName,
      graphToken,
      sharePointToken
    } = this.props;

    const graphTokenDecoded : any = jwtDecode(graphToken);
    const sharePointTokenDecoded : any = jwtDecode(sharePointToken);
    
    return (
      <section className={`${styles.scopesApp2WebPart} ${hasTeamsContext ? styles.teams : ''}`}>
        <div className={styles.welcome}>
          <img alt="" src={isDarkTheme ? require('../assets/welcome-dark.png') : require('../assets/welcome-light.png')} className={styles.welcomeImage} />
          <h2>Well done, {escape(userDisplayName)}!</h2>
          <div>{environmentMessage}</div>
        </div>
        <div>
          <h2>Token Information</h2>
          <div>Graph: <strong>{escape(graphResource)}</strong></div>
          
          <pre>app_displayname: {graphTokenDecoded.app_displayname}</pre>
          <pre>aud: {graphTokenDecoded.aud}</pre>
          <pre>scp: {graphTokenDecoded.scp}</pre>
          <pre>iss: {graphTokenDecoded.iss}</pre>
                   
          <div>SharePoint: <strong>{escape(sharePointResource)}</strong></div>

          <pre>app_displayname: {sharePointTokenDecoded.app_displayname}</pre>
          <pre>aud: {sharePointTokenDecoded.aud}</pre>
          <pre>scp: {sharePointTokenDecoded.scp}</pre>
          <pre>iss: {sharePointTokenDecoded.iss}</pre>


        </div>
      </section>
    );
  }
}
