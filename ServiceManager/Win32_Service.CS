namespace derIgel.ROOT.CIMV2 {
    using System;
    using System.ComponentModel;
    using System.Management;
    using System.Collections;
    using System.Globalization;
    using System.ComponentModel.Design.Serialization;
    using System.Reflection;
    
    
    // Functions ShouldSerialize<PropertyName> are functions used by VS property browser to check if a particular property has to be serialized. These functions are added for all ValueType properties ( properties of type Int32, BOOL etc.. which cannot be set to null). These functions uses Is<PropertyName>Null function. These functions are also used in the TypeConverter implementation for the properties to check for NULL value of property so that a empty value can be shown in Property browser in case of Drag and Drop in Visual studio.
    // Functions Is<PropertyName>Null() are functions . These functions are to be used to check if a property is NULL.
    // Functions Reset<PropertyName> are added for Nullable Read/Write properties. These functions are used in VS designere in property browser to set a property to NULL.
    // 	Every property added to the class for WMI property has attributes set to define its behaviour in Visual Studio designer and as well as to define a TypeConverter to be used.
    // DateTime Conversions are added for the class to convert DMTF date to System.DateTime and Vise-versa. Conversion from DMTF to System.DateTime conversion ignores the microseconds as System.DateTime doesn't have the microseconds part in it.
    // An Early Bound class generated for the WMI class.Win32_Service
    public class Service : System.ComponentModel.Component {
        
        // Private property to hold the name of WMI class which created this class.
        private static string CreatedClassName = "Win32_Service";
        
        // Property pointing to a embeded object to get System properties of the WMI object.
        private ManagementSystemProperties PrivateSystemProperties;
        
        // Underlying LateBound WMI object.
        private System.Management.ManagementObject PrivateLateBoundObject;
        
        // Member variable to store the autocommit behaviour for the class.
        private bool AutoCommitProp = true;
        
        // Private variable to hold the embedded property representing the instance.
        private System.Management.ManagementBaseObject embeddedObj;
        
        // The current WMI object used
        private System.Management.ManagementBaseObject curObj;
        
        // Flag to indicate if t	he instance is a embedded object.
        private bool isEmbedded = false;
        
        // Below are different flavours of constructors to initialize the instance with a WMI object.
        public Service() : 
                this(((System.Management.ManagementScope)(null)), ((System.Management.ManagementPath)(null)), ((System.Management.ObjectGetOptions)(null))) {
        }
        
        public Service(string keyName) : 
                this(((System.Management.ManagementScope)(null)), ((System.Management.ManagementPath)(new System.Management.ManagementPath(Service.ConstructPath(keyName)))), ((System.Management.ObjectGetOptions)(null))) {
        }
        
        public Service(System.Management.ManagementScope mgmtScope, string keyName) : 
                this(((System.Management.ManagementScope)(mgmtScope)), ((System.Management.ManagementPath)(new System.Management.ManagementPath(Service.ConstructPath(keyName)))), ((System.Management.ObjectGetOptions)(null))) {
        }
        
        public Service(System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions) : 
                this(((System.Management.ManagementScope)(null)), ((System.Management.ManagementPath)(path)), ((System.Management.ObjectGetOptions)(getOptions))) {
        }
        
        public Service(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path) : 
                this(((System.Management.ManagementScope)(mgmtScope)), ((System.Management.ManagementPath)(path)), ((System.Management.ObjectGetOptions)(null))) {
        }
        
        public Service(System.Management.ManagementPath path) : 
                this(((System.Management.ManagementScope)(null)), ((System.Management.ManagementPath)(path)), ((System.Management.ObjectGetOptions)(null))) {
        }
        
        public Service(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions) {
            if ((path != null)) {
                if ((CheckIfProperClass(mgmtScope, path, getOptions) != true)) {
                    throw new System.ArgumentException("Class name does not match.");
                }
            }
            PrivateLateBoundObject = new System.Management.ManagementObject(mgmtScope, path, getOptions);
            PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
            curObj = PrivateLateBoundObject;
        }
        
        public Service(System.Management.ManagementObject theObject) {
            if ((CheckIfProperClass(theObject.Scope, theObject.Path, theObject.Options) == true)) {
                PrivateLateBoundObject = theObject;
                PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
                curObj = PrivateLateBoundObject;
            }
            else {
                throw new System.ArgumentException("Class name does not match.");
            }
        }
        
        public Service(System.Management.ManagementBaseObject theObject) {
            if ((CheckIfProperClass(theObject) == true)) {
                embeddedObj = theObject;
                PrivateSystemProperties = new ManagementSystemProperties(theObject);
                curObj = embeddedObj;
                isEmbedded = true;
            }
            else {
                throw new System.ArgumentException("Class name does not match.");
            }
        }
        
        // Property returns the namespace of the WMI class.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string OriginatingNamespace {
            get {
                return "ROOT\\CIMV2";
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ManagementClassName {
            get {
                string strRet = CreatedClassName;
                if ((curObj != null)) {
                    if ((curObj.ClassPath != null)) {
                        strRet = ((string)(curObj["__CLASS"]));
                        if (((strRet == null) 
                                    || (strRet == System.String.Empty))) {
                            strRet = CreatedClassName;
                        }
                    }
                }
                return strRet;
            }
        }
        
        // Property pointing to a embeded object to get System properties of the WMI object.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ManagementSystemProperties SystemProperties {
            get {
                return PrivateSystemProperties;
            }
        }
        
        // Property returning the underlying LateBound object.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Management.ManagementBaseObject LateBoundObject {
            get {
                return curObj;
            }
        }
        
        // ManagementScope of the object.
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Management.ManagementScope Scope {
            get {
                if ((isEmbedded == false)) {
                    return PrivateLateBoundObject.Scope;
                }
                else {
                    return null;
                }
            }
            set {
                if ((isEmbedded == false)) {
                    PrivateLateBoundObject.Scope = value;
                }
            }
        }
        
        // Property to show the autocommit behaviour for the WMI object. If this is true then WMI object is saved to WMI then for change in every property (ie. Put is called after modification of a property).
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoCommit {
            get {
                return AutoCommitProp;
            }
            set {
                AutoCommitProp = value;
            }
        }
        
        // The ManagementPath of the underlying WMI object.
        [Browsable(true)]
        public System.Management.ManagementPath Path {
            get {
                if ((isEmbedded == false)) {
                    return PrivateLateBoundObject.Path;
                }
                else {
                    return null;
                }
            }
            set {
                if ((isEmbedded == false)) {
                    if ((CheckIfProperClass(null, value, null) != true)) {
                        throw new System.ArgumentException("Class name does not match.");
                    }
                    PrivateLateBoundObject.Path = value;
                }
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsAcceptPauseNull {
            get {
                if ((curObj["AcceptPause"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The AcceptPause property indicates whether the service can be paused.\nValues: TRU" +
"E or FALSE. A value of TRUE indicates the service can be paused.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool AcceptPause {
            get {
                if ((curObj["AcceptPause"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["AcceptPause"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsAcceptStopNull {
            get {
                if ((curObj["AcceptStop"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The AcceptStop property indicates whether the service can be stopped.\nValues: TRU" +
"E or FALSE. A value of TRUE indicates the service can be stopped.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool AcceptStop {
            get {
                if ((curObj["AcceptStop"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["AcceptStop"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Caption property is a short textual description (one-line string) of the obje" +
"ct.")]
        public string Caption {
            get {
                return ((string)(curObj["Caption"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsCheckPointNull {
            get {
                if ((curObj["CheckPoint"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The CheckPoint property specifies a value that the service increments periodically to report its progress during a lengthy start, stop, pause, or continue operation. For example, the service should increment this value as it completes each step of its initialization when it is starting up. The user interface program that invoked the operation on the service uses this value to track the progress of the service during a lengthy operation. This value is not valid and should be zero when the service does not have a start, stop, pause, or continue operation pending.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.UInt32 CheckPoint {
            get {
                if ((curObj["CheckPoint"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((System.UInt32)(curObj["CheckPoint"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("CreationClassName indicates the name of the class or the subclass used in the cre" +
"ation of an instance. When used with the other key properties of this class, thi" +
"s property allows all instances of this class and its subclasses to be uniquely " +
"identified.")]
        public string CreationClassName {
            get {
                return ((string)(curObj["CreationClassName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Description property provides a textual description of the object. ")]
        public string Description {
            get {
                return ((string)(curObj["Description"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDesktopInteractNull {
            get {
                if ((curObj["DesktopInteract"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The DesktopInteract property indicates whether the service can create or communic" +
"ate with windows on the desktop.\nValues: TRUE or FALSE. A value of TRUE indicate" +
"s the service can create or communicate with windows on the desktop.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool DesktopInteract {
            get {
                if ((curObj["DesktopInteract"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["DesktopInteract"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The DisplayName property indicates the display name of the service. This string has a maximum length of 256 characters. The name is case-preserved in the Service Control Manager. DisplayName comparisons are always case-insensitive. 
Constraints: Accepts the same value as the Name property.
Example: Atdisk.")]
        public string DisplayName {
            get {
                return ((string)(curObj["DisplayName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"If this service fails to start during startup, the ErrorControl property specifies the severity of the error. The value indicates the action taken by the startup program if failure occurs. All errors are logged by the computer system. The computer system does not notify the user of ""Ignore"" errors. With ""Normal"" errorsthe user is notified. With ""Severe"" errors, the system is restarted with the last-known-good configuration. Finally, on""Critical"" errors the system attempts to restart with a good configuration.")]
        public string ErrorControl {
            get {
                return ((string)(curObj["ErrorControl"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExitCodeNull {
            get {
                if ((curObj["ExitCode"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The ExitCode property specifies a Win32 error code defining any problems encountered in starting or stopping the service. This property is set to ERROR_SERVICE_SPECIFIC_ERROR (1066) when the error is unique to the service represented by this class, and information about the error is available in the ServiceSpecificExitCode member. The service sets this value to NO_ERROR when running, and again upon normal termination.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.UInt32 ExitCode {
            get {
                if ((curObj["ExitCode"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((System.UInt32)(curObj["ExitCode"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInstallDateNull {
            get {
                if ((curObj["InstallDate"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The InstallDate property is datetime value indicating when the object was install" +
"ed. A lack of a value does not indicate that the object is not installed.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.DateTime InstallDate {
            get {
                if ((curObj["InstallDate"] != null)) {
                    return ToDateTime(((string)(curObj["InstallDate"])));
                }
                else {
                    return ToDateTime(null);
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Name property uniquely identifies the service and provides an indication of t" +
"he functionality that is managed. This functionality is described in more detail" +
" in the object\'s Description property. ")]
        public string Name {
            get {
                return ((string)(curObj["Name"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The PathName property contains the fully qualified path to the service binary fil" +
"e that implements the service.\nExample: \\SystemRoot\\System32\\drivers\\afd.sys")]
        public string PathName {
            get {
                return ((string)(curObj["PathName"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsProcessIdNull {
            get {
                if ((curObj["ProcessId"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ProcessId property specifies the process identifier of the service.\nExample: " +
"324")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.UInt32 ProcessId {
            get {
                if ((curObj["ProcessId"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((System.UInt32)(curObj["ProcessId"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsServiceSpecificExitCodeNull {
            get {
                if ((curObj["ServiceSpecificExitCode"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The ServiceSpecificExitCode property specifies a service-specific error code for errors that occur while the service is either starting or stopping. The exit codes are defined by the service represented by this class. This value is only set when the ExitCodeproperty value is ERROR_SERVICE_SPECIFIC_ERROR, 1066.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.UInt32 ServiceSpecificExitCode {
            get {
                if ((curObj["ServiceSpecificExitCode"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((System.UInt32)(curObj["ServiceSpecificExitCode"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ServiceType property supplies the type of service provided to calling process" +
"es.")]
        public string ServiceType {
            get {
                return ((string)(curObj["ServiceType"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsStartedNull {
            get {
                if ((curObj["Started"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Started is a boolean indicating whether the service has been started (TRUE), or s" +
"topped (FALSE).")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool Started {
            get {
                if ((curObj["Started"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["Started"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The StartMode property indicates the start mode of the Win32 base service. ""Boot"" specifies a device driver started by the operating system loader. This value is valid only for driver services. ""System"" specifies a device driver started by the IoInitSystem function. This value is valid only for driver services. ""Automatic"" specifies a service to be started automatically by the service control manager during system startup. ""Manual"" specifies a service to be started by the service control manager when a process calls the StartService function. ""Disabled"" specifies a service that can no longer be started.")]
        public string StartMode {
            get {
                return ((string)(curObj["StartMode"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The StartName property indicates the account name under which the service runs. Depending on the service type, the account name may be in the form of ""DomainName\Username"".The service process will be logged using one of these two forms when it runs. If the account belongs to the built-in domain, "".\Username"" can be specified. If NULL is specified, the service will be logged on as the LocalSystem account. For kernel or system level drivers, StartName contains the driver object name (that is, \FileSystem\Rdr or \Driver\Xns) which the input and output (I/O) system uses to load the device driver. Additionally, if NULL is specified, the driver runs with a default object name created by the I/O system based on the service name.
Example: DWDOM\Admin.")]
        public string StartName {
            get {
                return ((string)(curObj["StartName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The State property indicates the current state of the base service.")]
        public string State {
            get {
                return ((string)(curObj["State"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The Status property is a string indicating the current status of the object. Various operational and non-operational statuses can be defined. Operational statuses are ""OK"", ""Degraded"" and ""Pred Fail"". ""Pred Fail"" indicates that an element may be functioning properly but predicting a failure in the near future. An example is a SMART-enabled hard drive. Non-operational statuses can also be specified. These are ""Error"", ""Starting"", ""Stopping"" and ""Service"". The latter, ""Service"", could apply during mirror-resilvering of a disk, reload of a user permissions list, or other administrative work. Not all such work is on-line, yet the managed element is neither ""OK"" nor in one of the other states.")]
        public string Status {
            get {
                return ((string)(curObj["Status"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The type name of the system that hosts this service")]
        public string SystemCreationClassName {
            get {
                return ((string)(curObj["SystemCreationClassName"]));
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The name of the system that hosts this service")]
        public string SystemName {
            get {
                return ((string)(curObj["SystemName"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTagIdNull {
            get {
                if ((curObj["TagId"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The TagId property specifies a unique tag value for this service in the group. A value of 0 indicates that the service has not been assigned a tag. A tag can be used for ordering service startup within a load order group by specifying a tag order vector in the registry located at: HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\GroupOrderList. Tags are only evaluated for Kernel Driver and File System Driver start type services that have ""Boot"" or ""System"" start modes.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.UInt32 TagId {
            get {
                if ((curObj["TagId"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((System.UInt32)(curObj["TagId"]));
            }
        }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsWaitHintNull {
            get {
                if ((curObj["WaitHint"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The WaitHint property specifies the estimated time required (in milliseconds) for a pending start, stop, pause, or continue operation. After the specified amount of time has elapsed, the service makes its next call to the SetServiceStatus function with either an incremented CheckPoint value or a change in CurrentState. If the amount of time specified by WaitHint passes, and CheckPoint has not been incremented, or the CurrentState has not changed, the service control manager or service control program assumes that an error has occurred.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.UInt32 WaitHint {
            get {
                if ((curObj["WaitHint"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((System.UInt32)(curObj["WaitHint"]));
            }
        }
        
        private bool CheckIfProperClass(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions OptionsParam) {
            if (((path != null) 
                        && (System.String.Compare(path.ClassName, ManagementClassName, true) == 0))) {
                return true;
            }
            else {
                return CheckIfProperClass(new System.Management.ManagementObject(mgmtScope, path, OptionsParam));
            }
        }
        
        private bool CheckIfProperClass(System.Management.ManagementBaseObject theObj) {
            if (((theObj != null) 
                        && (System.String.Compare(((string)(theObj["__CLASS"])), ManagementClassName, true) == 0))) {
                return true;
            }
            else {
                System.Array parentClasses = ((System.Array)(theObj["__DERIVATION"]));
                if ((parentClasses != null)) {
                    Int32 count = 0;
                    for (count = 0; (count < parentClasses.Length); count = (count + 1)) {
                        if ((System.String.Compare(((string)(parentClasses.GetValue(count))), ManagementClassName, true) == 0)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        private bool ShouldSerializeAcceptPause() {
            if ((IsAcceptPauseNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeAcceptStop() {
            if ((IsAcceptStopNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeCheckPoint() {
            if ((IsCheckPointNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeDesktopInteract() {
            if ((IsDesktopInteractNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeExitCode() {
            if ((IsExitCodeNull == false)) {
                return true;
            }
            return false;
        }
        
        static System.DateTime ToDateTime(string dmtfDate) {
            int year = System.DateTime.Now.Year;
            int month = 1;
            int day = 1;
            int hour = 0;
            int minute = 0;
            int second = 0;
            int millisec = 0;
            string dmtf = dmtfDate;
            string tempString = System.String.Empty;
            if (((System.String.Empty == dmtf) 
                        || (dmtf == null))) {
                return System.DateTime.MinValue;
            }
            if ((dmtf.Length != 25)) {
                return System.DateTime.MinValue;
            }
            tempString = dmtf.Substring(0, 4);
            if (("****" != tempString)) {
                year = System.Int32.Parse(tempString);
            }
            tempString = dmtf.Substring(4, 2);
            if (("**" != tempString)) {
                month = System.Int32.Parse(tempString);
            }
            tempString = dmtf.Substring(6, 2);
            if (("**" != tempString)) {
                day = System.Int32.Parse(tempString);
            }
            tempString = dmtf.Substring(8, 2);
            if (("**" != tempString)) {
                hour = System.Int32.Parse(tempString);
            }
            tempString = dmtf.Substring(10, 2);
            if (("**" != tempString)) {
                minute = System.Int32.Parse(tempString);
            }
            tempString = dmtf.Substring(12, 2);
            if (("**" != tempString)) {
                second = System.Int32.Parse(tempString);
            }
            tempString = dmtf.Substring(15, 3);
            if (("***" != tempString)) {
                millisec = System.Int32.Parse(tempString);
            }
            System.DateTime dateRet = new System.DateTime(year, month, day, hour, minute, second, millisec);
            return dateRet;
        }
        
        static string ToDMTFTime(System.DateTime dateParam) {
            string tempString = dateParam.Year.ToString();
            tempString = (tempString + dateParam.Month.ToString().PadLeft(2, '0'));
            tempString = (tempString + dateParam.Day.ToString().PadLeft(2, '0'));
            tempString = (tempString + dateParam.Hour.ToString().PadLeft(2, '0'));
            tempString = (tempString + dateParam.Minute.ToString().PadLeft(2, '0'));
            tempString = (tempString + dateParam.Second.ToString().PadLeft(2, '0'));
            tempString = (tempString + ".");
            tempString = (tempString + dateParam.Millisecond.ToString().PadLeft(3, '0'));
            tempString = (tempString + "000");
            System.TimeZone curZone = System.TimeZone.CurrentTimeZone;
            System.TimeSpan tickOffset = curZone.GetUtcOffset(dateParam);
            if ((tickOffset.Ticks >= 0)) {
                tempString = (tempString + "+");
                tempString = (tempString + ((tickOffset.Ticks / System.TimeSpan.TicksPerMinute)).ToString());
            }
            else {
                tempString = (tempString + "-");
                tempString = (tempString + ((tickOffset.Ticks / System.TimeSpan.TicksPerMinute)).ToString().Substring(1, 3));
            }
            return tempString;
        }
        
        private bool ShouldSerializeInstallDate() {
            if ((IsInstallDateNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeProcessId() {
            if ((IsProcessIdNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeServiceSpecificExitCode() {
            if ((IsServiceSpecificExitCodeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeStarted() {
            if ((IsStartedNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeTagId() {
            if ((IsTagIdNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeWaitHint() {
            if ((IsWaitHintNull == false)) {
                return true;
            }
            return false;
        }
        
        [Browsable(true)]
        public void CommitObject() {
            if ((isEmbedded == false)) {
                PrivateLateBoundObject.Put();
            }
        }
        
        private static string ConstructPath(string keyName) {
            string strPath = "ROOT\\CIMV2:Win32_Service";
            strPath = (strPath 
                        + (".Name=" 
                        + ("\"" 
                        + (keyName + "\""))));
            return strPath;
        }
        
        // Different flavours of GetInstances() help in enumerating instances of the WMI class.
        public static ServiceCollection GetInstances() {
            return GetInstances(((System.Management.ManagementScope)(null)), ((System.Management.EnumerationOptions)(null)));
        }
        
        public static ServiceCollection GetInstances(string condition) {
            return GetInstances(null, condition, null);
        }
        
        public static ServiceCollection GetInstances(System.String [] selectedProperties) {
            return GetInstances(null, null, selectedProperties);
        }
        
        public static ServiceCollection GetInstances(string condition, System.String [] selectedProperties) {
            return GetInstances(null, condition, selectedProperties);
        }
        
        public static ServiceCollection GetInstances(System.Management.ManagementScope mgmtScope, System.Management.EnumerationOptions enumOptions) {
            if ((mgmtScope == null)) {
                mgmtScope = new System.Management.ManagementScope();
                mgmtScope.Path.NamespacePath = "root\\CIMV2";
            }
            System.Management.ManagementPath pathObj = new System.Management.ManagementPath();
            pathObj.ClassName = "Win32_Service";
            pathObj.NamespacePath = "root\\CIMV2";
            System.Management.ManagementClass clsObject = new System.Management.ManagementClass(mgmtScope, pathObj, null);
            if ((enumOptions == null)) {
                enumOptions = new System.Management.EnumerationOptions();
                enumOptions.EnsureLocatable = true;
            }
            return new ServiceCollection(clsObject.GetInstances(enumOptions));
        }
        
        public static ServiceCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition) {
            return GetInstances(mgmtScope, condition, null);
        }
        
        public static ServiceCollection GetInstances(System.Management.ManagementScope mgmtScope, System.String [] selectedProperties) {
            return GetInstances(mgmtScope, null, selectedProperties);
        }
        
        public static ServiceCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition, System.String [] selectedProperties) {
            if ((mgmtScope == null)) {
                mgmtScope = new System.Management.ManagementScope();
                mgmtScope.Path.NamespacePath = "root\\CIMV2";
            }
            System.Management.ManagementObjectSearcher ObjectSearcher = new System.Management.ManagementObjectSearcher(mgmtScope, new SelectQuery("Win32_Service", condition, selectedProperties));
            System.Management.EnumerationOptions enumOptions = new System.Management.EnumerationOptions();
            enumOptions.EnsureLocatable = true;
            ObjectSearcher.Options = enumOptions;
            return new ServiceCollection(ObjectSearcher.Get());
        }
        
        public System.UInt32 Change(bool DesktopInteract, string DisplayName, System.Byte ErrorControl, string LoadOrderGroup, string[] LoadOrderGroupDependencies, string PathName, string[] ServiceDependencies, System.Byte ServiceType, string StartMode, string StartName, string StartPassword) {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                inParams = PrivateLateBoundObject.GetMethodParameters("Change");
                inParams["DesktopInteract"] = DesktopInteract;
                inParams["DisplayName"] = DisplayName;
                inParams["ErrorControl"] = ErrorControl;
                inParams["LoadOrderGroup"] = LoadOrderGroup;
                inParams["LoadOrderGroupDependencies"] = LoadOrderGroupDependencies;
                inParams["PathName"] = PathName;
                inParams["ServiceDependencies"] = ServiceDependencies;
                inParams["ServiceType"] = ServiceType;
                inParams["StartMode"] = StartMode;
                inParams["StartName"] = StartName;
                inParams["StartPassword"] = StartPassword;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Change", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public System.UInt32 ChangeStartMode(string StartMode) {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                inParams = PrivateLateBoundObject.GetMethodParameters("ChangeStartMode");
                inParams["StartMode"] = StartMode;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("ChangeStartMode", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public System.UInt32 Create(bool DesktopInteract, string DisplayName, System.Byte ErrorControl, string LoadOrderGroup, string[] LoadOrderGroupDependencies, string Name, string PathName, string[] ServiceDependencies, System.Byte ServiceType, string StartMode, string StartName, string StartPassword) {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                inParams = PrivateLateBoundObject.GetMethodParameters("Create");
                inParams["DesktopInteract"] = DesktopInteract;
                inParams["DisplayName"] = DisplayName;
                inParams["ErrorControl"] = ErrorControl;
                inParams["LoadOrderGroup"] = LoadOrderGroup;
                inParams["LoadOrderGroupDependencies"] = LoadOrderGroupDependencies;
                inParams["Name"] = Name;
                inParams["PathName"] = PathName;
                inParams["ServiceDependencies"] = ServiceDependencies;
                inParams["ServiceType"] = ServiceType;
                inParams["StartMode"] = StartMode;
                inParams["StartName"] = StartName;
                inParams["StartPassword"] = StartPassword;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Create", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public System.UInt32 Delete() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Delete", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public System.UInt32 InterrogateService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("InterrogateService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public System.UInt32 PauseService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("PauseService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public System.UInt32 ResumeService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("ResumeService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public System.UInt32 StartService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("StartService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public System.UInt32 StopService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("StopService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        public System.UInt32 UserControlService(System.Byte ControlCode) {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                inParams = PrivateLateBoundObject.GetMethodParameters("UserControlService");
                inParams["ControlCode"] = ControlCode;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("UserControlService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        // Enumerator implementation for enumerating instances of the class.
        public class ServiceCollection : object, ICollection {
            
            private ManagementObjectCollection ObjectCollection;
            
            public ServiceCollection(ManagementObjectCollection objCollection) {
                ObjectCollection = objCollection;
            }
            
            public int Count {
                get {
                    return ObjectCollection.Count;
                }
            }
            
            public bool IsSynchronized {
                get {
                    return ObjectCollection.IsSynchronized;
                }
            }
            
            public object SyncRoot {
                get {
                    return this;
                }
            }
            
            public void CopyTo(System.Array array, int index) {
                ObjectCollection.CopyTo(array, index);
                int nCtr;
                for (nCtr = 0; (nCtr < array.Length); nCtr = (nCtr + 1)) {
                    array.SetValue(new Service(((System.Management.ManagementObject)(array.GetValue(nCtr)))), nCtr);
                }
            }
            
            public System.Collections.IEnumerator GetEnumerator() {
                return new ServiceEnumerator(ObjectCollection.GetEnumerator());
            }
            
            public class ServiceEnumerator : object, System.Collections.IEnumerator {
                
                private ManagementObjectCollection.ManagementObjectEnumerator ObjectEnumerator;
                
                public ServiceEnumerator(ManagementObjectCollection.ManagementObjectEnumerator objEnum) {
                    ObjectEnumerator = objEnum;
                }
                
                public object Current {
                    get {
                        return new Service(((System.Management.ManagementObject)(ObjectEnumerator.Current)));
                    }
                }
                
                public bool MoveNext() {
                    return ObjectEnumerator.MoveNext();
                }
                
                public void Reset() {
                    ObjectEnumerator.Reset();
                }
            }
        }
        
        // TypeConverter to handle null values for ValueType properties
        public class WMIValueTypeConverter : TypeConverter {
            
            private TypeConverter baseConverter;
            
            public WMIValueTypeConverter(System.Type baseType) {
                baseConverter = TypeDescriptor.GetConverter(baseType);
            }
            
            public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type srcType) {
                return baseConverter.CanConvertFrom(context, srcType);
            }
            
            public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType) {
                return baseConverter.CanConvertTo(context, destinationType);
            }
            
            public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
                return baseConverter.ConvertFrom(context, culture, value);
            }
            
            public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext context, System.Collections.IDictionary dictionary) {
                return baseConverter.CreateInstance(context, dictionary);
            }
            
            public override bool GetCreateInstanceSupported(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetCreateInstanceSupported(context);
            }
            
            public override PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext context, object value, System.Attribute[] attributeVar) {
                return baseConverter.GetProperties(context, value, attributeVar);
            }
            
            public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetPropertiesSupported(context);
            }
            
            public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetStandardValues(context);
            }
            
            public override bool GetStandardValuesExclusive(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetStandardValuesExclusive(context);
            }
            
            public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetStandardValuesSupported(context);
            }
            
            public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType) {
                if ((context != null)) {
                    if ((context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false)) {
                        return "";
                    }
                }
                return baseConverter.ConvertTo(context, culture, value, destinationType);
            }
        }
        
        // Embedded class to represent WMI system Properties.
        [TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ManagementSystemProperties {
            
            private System.Management.ManagementBaseObject PrivateLateBoundObject;
            
            public ManagementSystemProperties(System.Management.ManagementBaseObject ManagedObject) {
                PrivateLateBoundObject = ManagedObject;
            }
            
            [Browsable(true)]
            public int GENUS {
                get {
                    return ((int)(PrivateLateBoundObject["__GENUS"]));
                }
            }
            
            [Browsable(true)]
            public string CLASS {
                get {
                    return ((string)(PrivateLateBoundObject["__CLASS"]));
                }
            }
            
            [Browsable(true)]
            public string SUPERCLASS {
                get {
                    return ((string)(PrivateLateBoundObject["__SUPERCLASS"]));
                }
            }
            
            [Browsable(true)]
            public string DYNASTY {
                get {
                    return ((string)(PrivateLateBoundObject["__DYNASTY"]));
                }
            }
            
            [Browsable(true)]
            public string RELPATH {
                get {
                    return ((string)(PrivateLateBoundObject["__RELPATH"]));
                }
            }
            
            [Browsable(true)]
            public int PROPERTY_COUNT {
                get {
                    return ((int)(PrivateLateBoundObject["__PROPERTY_COUNT"]));
                }
            }
            
            [Browsable(true)]
            public string[] DERIVATION {
                get {
                    return ((string[])(PrivateLateBoundObject["__DERIVATION"]));
                }
            }
            
            [Browsable(true)]
            public string SERVER {
                get {
                    return ((string)(PrivateLateBoundObject["__SERVER"]));
                }
            }
            
            [Browsable(true)]
            public string NAMESPACE {
                get {
                    return ((string)(PrivateLateBoundObject["__NAMESPACE"]));
                }
            }
            
            [Browsable(true)]
            public string PATH {
                get {
                    return ((string)(PrivateLateBoundObject["__PATH"]));
                }
            }
        }
    }
}
