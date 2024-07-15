import * as React from 'react';
import styles from './ScopesApp1WebPart.module.scss';
import type { IScopesApp1WebPartProps } from './IScopesApp1WebPartProps';
import { escape } from '@microsoft/sp-lodash-subset';
import { jwtDecode } from "jwt-decode";

export default class ScopesApp1WebPart extends React.Component<IScopesApp1WebPartProps, {}> {
  public render(): React.ReactElement<IScopesApp1WebPartProps> {
    const {
      graphResource,
      isDarkTheme,
      environmentMessage,
      hasTeamsContext,
      userDisplayName,
      graphToken
    } = this.props;

    const graphTokenDecoded : any = jwtDecode(graphToken);

    return (
      <section className={`${styles.scopesApp1WebPart} ${hasTeamsContext ? styles.teams : ''}`}>
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
        </div>        
      </section>
    );
  }
}
