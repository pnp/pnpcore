import * as React from 'react';
import * as ReactDom from 'react-dom';
import { BaseDialog, IDialogConfiguration } from '@microsoft/sp-dialog';
import { DialogContent, DialogFooter, DefaultButton, PrimaryButton } from 'office-ui-fabric-react';
import { Banner } from '../../../sharedComponents/banner/Banner';

export interface ICustomDialogProps {
    title: string;
    message: string;
    onClose?: () => void;
}

const CustomDialog = (props: ICustomDialogProps) => {
    return <DialogContent
        title={props.title}
        onDismiss={props.onClose}
        showCloseButton={true}
    >
        <Banner message={props.message} />
        <DialogFooter>
            <DefaultButton text='Cancel' title='Cancel' onClick={props.onClose} />
            <PrimaryButton text='OK' title='OK' onClick={props.onClose} />
        </DialogFooter>
    </DialogContent>;
};

export class CustomCommandDialog extends BaseDialog {
    public message: string;

    public render(): void {
        ReactDom.render(<CustomDialog
            title='Custom command PnP Core SDK TEST'
            onClose={this.close}
            message={this.message}
        />, this.domElement);
    }

    public getConfig(): IDialogConfiguration {
        return {
            isBlocking: false
        };
    }

    protected onAfterClose(): void {
        super.onAfterClose();

        // Clean up the element for the next dialog
        ReactDom.unmountComponentAtNode(this.domElement);
    }
}