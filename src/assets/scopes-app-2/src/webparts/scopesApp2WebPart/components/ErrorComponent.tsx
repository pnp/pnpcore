import * as React from 'react';
import styles from './ErrorComponent.module.scss';
import type { IErrorComponentProps } from './IErrorComponentProps';
import { Icon } from '@fluentui/react/lib/Icon';
import { FontSizes } from '@fluentui/theme';

const Sad = () => <Icon iconName="SadSolid" />;


const ErrorComponent: React.FunctionComponent<IErrorComponentProps> = (props: IErrorComponentProps) => {

  const {
    error,
  } = props;

  return (
    <section className={`${styles.errorComponent}`}>
      <div>
        <div style={{ fontSize: FontSizes.size16 }} className={`${styles.error}`}> <Sad /> {error.errorMessage}</div>
      </div>
    </section>
  );
}

export default ErrorComponent;