﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18034
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ShareMetro.ServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="UserInfo", Namespace="http://schemas.datacontract.org/2004/07/ShareWare")]
    [System.SerializableAttribute()]
    public partial class UserInfo : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Drawing.Bitmap ImageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ImageHashField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsMaleField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string MicroBlogField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NickNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PasswordField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string QQField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string SignatureField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UserNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Drawing.Bitmap Image {
            get {
                return this.ImageField;
            }
            set {
                if ((object.ReferenceEquals(this.ImageField, value) != true)) {
                    this.ImageField = value;
                    this.RaisePropertyChanged("Image");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ImageHash {
            get {
                return this.ImageHashField;
            }
            set {
                if ((object.ReferenceEquals(this.ImageHashField, value) != true)) {
                    this.ImageHashField = value;
                    this.RaisePropertyChanged("ImageHash");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsMale {
            get {
                return this.IsMaleField;
            }
            set {
                if ((this.IsMaleField.Equals(value) != true)) {
                    this.IsMaleField = value;
                    this.RaisePropertyChanged("IsMale");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string MicroBlog {
            get {
                return this.MicroBlogField;
            }
            set {
                if ((object.ReferenceEquals(this.MicroBlogField, value) != true)) {
                    this.MicroBlogField = value;
                    this.RaisePropertyChanged("MicroBlog");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string NickName {
            get {
                return this.NickNameField;
            }
            set {
                if ((object.ReferenceEquals(this.NickNameField, value) != true)) {
                    this.NickNameField = value;
                    this.RaisePropertyChanged("NickName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Password {
            get {
                return this.PasswordField;
            }
            set {
                if ((object.ReferenceEquals(this.PasswordField, value) != true)) {
                    this.PasswordField = value;
                    this.RaisePropertyChanged("Password");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string QQ {
            get {
                return this.QQField;
            }
            set {
                if ((object.ReferenceEquals(this.QQField, value) != true)) {
                    this.QQField = value;
                    this.RaisePropertyChanged("QQ");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Signature {
            get {
                return this.SignatureField;
            }
            set {
                if ((object.ReferenceEquals(this.SignatureField, value) != true)) {
                    this.SignatureField = value;
                    this.RaisePropertyChanged("Signature");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string UserName {
            get {
                return this.UserNameField;
            }
            set {
                if ((object.ReferenceEquals(this.UserNameField, value) != true)) {
                    this.UserNameField = value;
                    this.RaisePropertyChanged("UserName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="OnlineUserInfo", Namespace="http://schemas.datacontract.org/2004/07/ShareWare")]
    [System.SerializableAttribute()]
    public partial class OnlineUserInfo : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ImageHashField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NickNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UserNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ImageHash {
            get {
                return this.ImageHashField;
            }
            set {
                if ((object.ReferenceEquals(this.ImageHashField, value) != true)) {
                    this.ImageHashField = value;
                    this.RaisePropertyChanged("ImageHash");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string NickName {
            get {
                return this.NickNameField;
            }
            set {
                if ((object.ReferenceEquals(this.NickNameField, value) != true)) {
                    this.NickNameField = value;
                    this.RaisePropertyChanged("NickName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string UserName {
            get {
                return this.UserNameField;
            }
            set {
                if ((object.ReferenceEquals(this.UserNameField, value) != true)) {
                    this.UserNameField = value;
                    this.RaisePropertyChanged("UserName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference.IShareService", CallbackContract=typeof(ShareMetro.ServiceReference.IShareServiceCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
    public interface IShareService {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/TickTack")]
        void TickTack();
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/TickTack")]
        System.Threading.Tasks.Task TickTackAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/OldLogin", ReplyAction="http://tempuri.org/IShareService/OldLoginResponse")]
        int OldLogin(string userName, string passWord, string mac);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/OldLogin", ReplyAction="http://tempuri.org/IShareService/OldLoginResponse")]
        System.Threading.Tasks.Task<int> OldLoginAsync(string userName, string passWord, string mac);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/Login", ReplyAction="http://tempuri.org/IShareService/LoginResponse")]
        int Login(string mac);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/Login", ReplyAction="http://tempuri.org/IShareService/LoginResponse")]
        System.Threading.Tasks.Task<int> LoginAsync(string mac);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsTerminating=true, Action="http://tempuri.org/IShareService/Logout")]
        void Logout();
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, IsTerminating=true, Action="http://tempuri.org/IShareService/Logout")]
        System.Threading.Tasks.Task LogoutAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/UploadShareInfo", ReplyAction="http://tempuri.org/IShareService/UploadShareInfoResponse")]
        bool UploadShareInfo(System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> fileList);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/UploadShareInfo", ReplyAction="http://tempuri.org/IShareService/UploadShareInfoResponse")]
        System.Threading.Tasks.Task<bool> UploadShareInfoAsync(System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> fileList);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/DownloadShareInfo", ReplyAction="http://tempuri.org/IShareService/DownloadShareInfoResponse")]
        System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> DownloadShareInfo();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/DownloadShareInfo", ReplyAction="http://tempuri.org/IShareService/DownloadShareInfoResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer>> DownloadShareInfoAsync();
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RemoveOldFile")]
        void RemoveOldFile(System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> fileList);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RemoveOldFile")]
        System.Threading.Tasks.Task RemoveOldFileAsync(System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> fileList);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RemoveNotExistShreFile")]
        void RemoveNotExistShreFile(string hash);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RemoveNotExistShreFile")]
        System.Threading.Tasks.Task RemoveNotExistShreFileAsync(string hash);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RemoveNotExistShreFileList")]
        void RemoveNotExistShreFileList(System.Collections.Generic.List<string> hashList);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RemoveNotExistShreFileList")]
        System.Threading.Tasks.Task RemoveNotExistShreFileListAsync(System.Collections.Generic.List<string> hashList);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/SearchFile", ReplyAction="http://tempuri.org/IShareService/SearchFileResponse")]
        System.Collections.Generic.List<ShareWare.ShareFile.FileInfoData> SearchFile(System.Collections.Generic.List<string> nameList, bool mustOnline);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/SearchFile", ReplyAction="http://tempuri.org/IShareService/SearchFileResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.List<ShareWare.ShareFile.FileInfoData>> SearchFileAsync(System.Collections.Generic.List<string> nameList, bool mustOnline);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/DownloadRequest", ReplyAction="http://tempuri.org/IShareService/DownloadRequestResponse")]
        int DownloadRequest(string hash, int nPort);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/DownloadRequest", ReplyAction="http://tempuri.org/IShareService/DownloadRequestResponse")]
        System.Threading.Tasks.Task<int> DownloadRequestAsync(string hash, int nPort);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/UploadImage")]
        void UploadImage(System.Drawing.Bitmap image);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/UploadImage")]
        System.Threading.Tasks.Task UploadImageAsync(System.Drawing.Bitmap image);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/DownloadUserImage", ReplyAction="http://tempuri.org/IShareService/DownloadUserImageResponse")]
        System.Drawing.Bitmap DownloadUserImage(string name);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/DownloadUserImage", ReplyAction="http://tempuri.org/IShareService/DownloadUserImageResponse")]
        System.Threading.Tasks.Task<System.Drawing.Bitmap> DownloadUserImageAsync(string name);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RequestConversation")]
        void RequestConversation(string userName, int localPort);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RequestConversation")]
        System.Threading.Tasks.Task RequestConversationAsync(string userName, int localPort);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RequestOpenShareFolder")]
        void RequestOpenShareFolder(string userName, int localPort);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RequestOpenShareFolder")]
        System.Threading.Tasks.Task RequestOpenShareFolderAsync(string userName, int localPort);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/SendChatRoomMessage")]
        void SendChatRoomMessage(string msg);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/SendChatRoomMessage")]
        System.Threading.Tasks.Task SendChatRoomMessageAsync(string msg);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/DownloadUserInfo", ReplyAction="http://tempuri.org/IShareService/DownloadUserInfoResponse")]
        ShareMetro.ServiceReference.UserInfo DownloadUserInfo(int userId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/DownloadUserInfo", ReplyAction="http://tempuri.org/IShareService/DownloadUserInfoResponse")]
        System.Threading.Tasks.Task<ShareMetro.ServiceReference.UserInfo> DownloadUserInfoAsync(int userId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/ChangeUserInfo", ReplyAction="http://tempuri.org/IShareService/ChangeUserInfoResponse")]
        bool ChangeUserInfo(ShareMetro.ServiceReference.UserInfo userInfo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/ChangeUserInfo", ReplyAction="http://tempuri.org/IShareService/ChangeUserInfoResponse")]
        System.Threading.Tasks.Task<bool> ChangeUserInfoAsync(ShareMetro.ServiceReference.UserInfo userInfo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/ChangePassword", ReplyAction="http://tempuri.org/IShareService/ChangePasswordResponse")]
        bool ChangePassword(string oldPassword, string newPassword);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IShareService/ChangePassword", ReplyAction="http://tempuri.org/IShareService/ChangePasswordResponse")]
        System.Threading.Tasks.Task<bool> ChangePasswordAsync(string oldPassword, string newPassword);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IShareServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/DownloadPerformance")]
        void DownloadPerformance(string szHash, string szIp, int nPort);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/NewUser")]
        void NewUser(ShareMetro.ServiceReference.OnlineUserInfo user);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/UserLeave")]
        void UserLeave(string name);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/RefreshUserList")]
        void RefreshUserList(System.Collections.Generic.List<ShareMetro.ServiceReference.OnlineUserInfo> userList);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/ConversationPerformance")]
        void ConversationPerformance(string remoteIp, int remotePort);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/OpenShareFolderPerfromance")]
        void OpenShareFolderPerfromance(string nickName, string remoteIp, int remotePort);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IShareService/ReceiveChatRoomMessage")]
        void ReceiveChatRoomMessage(string msg, string userName, string nickName);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IShareServiceChannel : ShareMetro.ServiceReference.IShareService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ShareServiceClient : System.ServiceModel.DuplexClientBase<ShareMetro.ServiceReference.IShareService>, ShareMetro.ServiceReference.IShareService {
        
        public ShareServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public ShareServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public ShareServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ShareServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ShareServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public void TickTack() {
            base.Channel.TickTack();
        }
        
        public System.Threading.Tasks.Task TickTackAsync() {
            return base.Channel.TickTackAsync();
        }
        
        public int OldLogin(string userName, string passWord, string mac) {
            return base.Channel.OldLogin(userName, passWord, mac);
        }
        
        public System.Threading.Tasks.Task<int> OldLoginAsync(string userName, string passWord, string mac) {
            return base.Channel.OldLoginAsync(userName, passWord, mac);
        }
        
        public int Login(string mac) {
            return base.Channel.Login(mac);
        }
        
        public System.Threading.Tasks.Task<int> LoginAsync(string mac) {
            return base.Channel.LoginAsync(mac);
        }
        
        public void Logout() {
            base.Channel.Logout();
        }
        
        public System.Threading.Tasks.Task LogoutAsync() {
            return base.Channel.LogoutAsync();
        }
        
        public bool UploadShareInfo(System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> fileList) {
            return base.Channel.UploadShareInfo(fileList);
        }
        
        public System.Threading.Tasks.Task<bool> UploadShareInfoAsync(System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> fileList) {
            return base.Channel.UploadShareInfoAsync(fileList);
        }
        
        public System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> DownloadShareInfo() {
            return base.Channel.DownloadShareInfo();
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer>> DownloadShareInfoAsync() {
            return base.Channel.DownloadShareInfoAsync();
        }
        
        public void RemoveOldFile(System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> fileList) {
            base.Channel.RemoveOldFile(fileList);
        }
        
        public System.Threading.Tasks.Task RemoveOldFileAsync(System.Collections.Generic.List<ShareWare.ShareFile.FileInfoTransfer> fileList) {
            return base.Channel.RemoveOldFileAsync(fileList);
        }
        
        public void RemoveNotExistShreFile(string hash) {
            base.Channel.RemoveNotExistShreFile(hash);
        }
        
        public System.Threading.Tasks.Task RemoveNotExistShreFileAsync(string hash) {
            return base.Channel.RemoveNotExistShreFileAsync(hash);
        }
        
        public void RemoveNotExistShreFileList(System.Collections.Generic.List<string> hashList) {
            base.Channel.RemoveNotExistShreFileList(hashList);
        }
        
        public System.Threading.Tasks.Task RemoveNotExistShreFileListAsync(System.Collections.Generic.List<string> hashList) {
            return base.Channel.RemoveNotExistShreFileListAsync(hashList);
        }
        
        public System.Collections.Generic.List<ShareWare.ShareFile.FileInfoData> SearchFile(System.Collections.Generic.List<string> nameList, bool mustOnline) {
            return base.Channel.SearchFile(nameList, mustOnline);
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.List<ShareWare.ShareFile.FileInfoData>> SearchFileAsync(System.Collections.Generic.List<string> nameList, bool mustOnline) {
            return base.Channel.SearchFileAsync(nameList, mustOnline);
        }
        
        public int DownloadRequest(string hash, int nPort) {
            return base.Channel.DownloadRequest(hash, nPort);
        }
        
        public System.Threading.Tasks.Task<int> DownloadRequestAsync(string hash, int nPort) {
            return base.Channel.DownloadRequestAsync(hash, nPort);
        }
        
        public void UploadImage(System.Drawing.Bitmap image) {
            base.Channel.UploadImage(image);
        }
        
        public System.Threading.Tasks.Task UploadImageAsync(System.Drawing.Bitmap image) {
            return base.Channel.UploadImageAsync(image);
        }
        
        public System.Drawing.Bitmap DownloadUserImage(string name) {
            return base.Channel.DownloadUserImage(name);
        }
        
        public System.Threading.Tasks.Task<System.Drawing.Bitmap> DownloadUserImageAsync(string name) {
            return base.Channel.DownloadUserImageAsync(name);
        }
        
        public void RequestConversation(string userName, int localPort) {
            base.Channel.RequestConversation(userName, localPort);
        }
        
        public System.Threading.Tasks.Task RequestConversationAsync(string userName, int localPort) {
            return base.Channel.RequestConversationAsync(userName, localPort);
        }
        
        public void RequestOpenShareFolder(string userName, int localPort) {
            base.Channel.RequestOpenShareFolder(userName, localPort);
        }
        
        public System.Threading.Tasks.Task RequestOpenShareFolderAsync(string userName, int localPort) {
            return base.Channel.RequestOpenShareFolderAsync(userName, localPort);
        }
        
        public void SendChatRoomMessage(string msg) {
            base.Channel.SendChatRoomMessage(msg);
        }
        
        public System.Threading.Tasks.Task SendChatRoomMessageAsync(string msg) {
            return base.Channel.SendChatRoomMessageAsync(msg);
        }
        
        public ShareMetro.ServiceReference.UserInfo DownloadUserInfo(int userId) {
            return base.Channel.DownloadUserInfo(userId);
        }
        
        public System.Threading.Tasks.Task<ShareMetro.ServiceReference.UserInfo> DownloadUserInfoAsync(int userId) {
            return base.Channel.DownloadUserInfoAsync(userId);
        }
        
        public bool ChangeUserInfo(ShareMetro.ServiceReference.UserInfo userInfo) {
            return base.Channel.ChangeUserInfo(userInfo);
        }
        
        public System.Threading.Tasks.Task<bool> ChangeUserInfoAsync(ShareMetro.ServiceReference.UserInfo userInfo) {
            return base.Channel.ChangeUserInfoAsync(userInfo);
        }
        
        public bool ChangePassword(string oldPassword, string newPassword) {
            return base.Channel.ChangePassword(oldPassword, newPassword);
        }
        
        public System.Threading.Tasks.Task<bool> ChangePasswordAsync(string oldPassword, string newPassword) {
            return base.Channel.ChangePasswordAsync(oldPassword, newPassword);
        }
    }
}
