import { override } from '@microsoft/decorators';
import { Log } from '@microsoft/sp-core-library';
import {
  BaseListViewCommandSet,
  Command,
  IListViewCommandSetListViewUpdatedParameters,
  IListViewCommandSetExecuteEventParameters
} from '@microsoft/sp-listview-extensibility';
import { find } from '@microsoft/sp-lodash-subset';
import { CustomCommandDialog } from './components/CustomDialog';

/**
 * If your command set uses the ClientSideComponentProperties JSON input,
 * it will be deserialized into the BaseExtension.properties object.
 * You can define an interface to describe it.
 */
export interface IPnPCoreSdkTestCommandCommandSetProperties {

}

const LOG_SOURCE: string = 'PnPCoreSdkTestCommandCommandSet';

export default class PnPCoreSdkTestCommandCommandSet extends BaseListViewCommandSet<IPnPCoreSdkTestCommandCommandSetProperties> {

  @override
  public onInit(): Promise<void> {
    Log.info(LOG_SOURCE, 'Initialized PnPCoreSdkTestCommandCommandSet');
    return Promise.resolve();
  }

  @override
  public onListViewUpdated(event: IListViewCommandSetListViewUpdatedParameters): void {
    const pnpCmd01: Command = this.tryGetCommand('PNP_CMD_01');
    if (pnpCmd01) {
      // This command should be hidden unless exactly one row is selected.
      pnpCmd01.visible = event.selectedRows.length === 1;
    }

    const pnpCmd02: Command = this.tryGetCommand('PNP_CMD_02');
    if (pnpCmd02) {
      // This command should be hidden unless more than one row is selected.
      pnpCmd02.visible = event.selectedRows.length > 1;
    }
  }

  private _getSelectedItemTitle(event: IListViewCommandSetExecuteEventParameters): string {
    if (!event.selectedRows || event.selectedRows.length !== 1) {
      return '';
    }

    const titleField = find(event.selectedRows[0].fields, f => f.internalName === 'FileLeafRef' || f.internalName === 'Title');
    if (!titleField) {
      return '';
    }

    return event.selectedRows[0].getValue(titleField);
  }

  @override
  public onExecute(event: IListViewCommandSetExecuteEventParameters): void {
    const selectedItemTitle = this._getSelectedItemTitle(event);
    const dialog = new CustomCommandDialog();
    switch (event.itemId) {
      case 'PNP_CMD_01':
        dialog.message = `You selected ${selectedItemTitle}`;
        dialog.show();
        break;
      case 'PNP_CMD_02':
        dialog.message = `You selected multiple items`;
        dialog.show();
        break;
      default:
        throw new Error('Unknown command');
    }
  }
}
