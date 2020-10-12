import * as React from 'react';
import { Logo } from '../logo/Logo';
import styles from './Banner.module.scss';

export interface IBannerProps {
    message?: string;
}

export const Banner = (props: IBannerProps) => {
    return (
        <div className={styles.pnPCoreSdkBanner}>
            <div className={`${styles.banner}`}>
                <Logo />
                <div>
                    {
                        props.message ||
                        <>Check out the docs at <a href="https://aka.ms/pnp/coresdk/docs">aka.ms/pnp/coresdk/docs</a></>
                    }
                </div>
            </div>
        </div>
    );
};