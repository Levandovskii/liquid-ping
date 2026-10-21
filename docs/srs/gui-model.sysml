package LiquidPingGui {
    doc /* Модель интерфейса Liquid Ping: информационная структура экранов и навигация между ними. */

    private import ScalarValues::*;

    part def UiElement {
        attribute caption : String;
    }

    part def Panel :> UiElement;
    part def Button :> UiElement;
    part def TextField :> UiElement;
    part def Dropdown :> UiElement;
    part def Checkbox :> UiElement;
    part def Toggle :> UiElement;
    part def Slider :> UiElement;
    part def SegmentedControl :> UiElement;
    part def ProgressBar :> UiElement;
    part def Table :> UiElement;
    part def Chart :> UiElement;
    part def Label :> UiElement;
    part def Tabs :> UiElement;

    part def Screen;

    part def HostCard :> Panel {
        part statusIndicator : Label;
        part nameLabel : Label;
        part latencyLabel : Label;
        part lossLabel : Label;
    }

    part def HostList :> Panel {
        part hostCard : HostCard [1..*];
    }

    part def MetricTile :> Panel {
        part titleLabel : Label;
        part valueLabel : Label;
    }

    part def Toolbar :> Panel {
        part modeSwitch : SegmentedControl;
        part settingsButton : Button;
        part themeButton : Button;
    }

    part def HostsToolbar :> Toolbar {
        part checkButton : Button;
        part startStopButton : Button;
        part intervalSelector : Dropdown;
        part searchField : TextField;
        part addHostButton : Button;
    }

    part def OverviewTab :> Panel {
        part metricTile : MetricTile [4];
        part latencyChart : Chart;
        part trafficLabel : Label;
    }

    part def PortsTab :> Panel {
        part portSetSelector : Dropdown;
        part scanButton : Button;
        part cancelButton : Button;
        part progressBar : ProgressBar;
        part resultTable : Table;
    }

    part def DetailsPanel :> Panel {
        part headerLabel : Label;
        part statusLabel : Label;
        part tabs : Tabs;
        part overviewTab : OverviewTab;
        part portsTab : PortsTab;
    }

    part def StatusBar :> Panel {
        part monitoringLabel : Label;
        part trafficLabel : Label;
    }

    part def HostsScreen :> Screen {
        part toolbar : HostsToolbar;
        part hostList : HostList;
        part detailsPanel : DetailsPanel;
        part statusBar : StatusBar;
    }

    part def LanScreen :> Screen {
        part toolbar : Toolbar;
        part subnetSelector : Dropdown;
        part scanButton : Button;
        part cancelButton : Button;
        part progressBar : ProgressBar;
        part resultTable : Table;
        part addToMonitoringButton : Button [0..*];
        part statusLabel : Label;
    }

    part def AddHostDialog :> Screen {
        part nameField : TextField;
        part addressField : TextField;
        part icmpCheckbox : Checkbox;
        part tcpCheckbox : Checkbox;
        part tcpPortField : TextField;
        part httpCheckbox : Checkbox;
        part httpUrlField : TextField;
        part cancelButton : Button;
        part saveButton : Button;
    }

    part def SettingsDialog :> Screen {
        part themeSelector : SegmentedControl;
        part languageSelector : SegmentedControl;
        part intervalSlider : Slider;
        part icmpTimeoutField : TextField;
        part tcpTimeoutField : TextField;
        part httpTimeoutField : TextField;
        part portTimeoutField : TextField;
        part chartWindowField : TextField;
        part notificationsToggle : Toggle;
        part trayToggle : Toggle;
        part closeButton : Button;
    }

    item def SelectHost;
    item def ShowOverview;
    item def ShowPorts;
    item def OpenLan;
    item def OpenHosts;
    item def OpenAddHost;
    item def OpenSettings;
    item def SaveHost;
    item def CancelDialog;
    item def CloseSettings;

    state def Navigation {
        entry; then hosts;

        state hosts {
            entry; then noSelection;

            state noSelection;
            state overview;
            state ports;

            transition selectFirstHost first noSelection accept SelectHost then overview;
            transition toPorts first overview accept ShowPorts then ports;
            transition toOverview first ports accept ShowOverview then overview;
        }

        state lan;
        state addHost;
        state settingsFromHosts;
        state settingsFromLan;

        transition hostsToLan first hosts accept OpenLan then lan;
        transition lanToHosts first lan accept OpenHosts then hosts;
        transition hostsToAddHost first hosts accept OpenAddHost then addHost;
        transition addHostSaved first addHost accept SaveHost then hosts;
        transition addHostCancelled first addHost accept CancelDialog then hosts;
        transition hostsToSettings first hosts accept OpenSettings then settingsFromHosts;
        transition settingsToHosts first settingsFromHosts accept CloseSettings then hosts;
        transition lanToSettings first lan accept OpenSettings then settingsFromLan;
        transition settingsToLan first settingsFromLan accept CloseSettings then lan;
    }

    part def LiquidPingGui {
        part hostsScreen : HostsScreen;
        part lanScreen : LanScreen;
        part addHostDialog : AddHostDialog;
        part settingsDialog : SettingsDialog;
        exhibit state navigation : Navigation;
    }
}
