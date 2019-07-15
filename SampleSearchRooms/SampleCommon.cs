// Copyright (c) Microsoft Corporation.  All rights reserved. 


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.PersistentChat;
using Microsoft.Rtc.Collaboration.PersistentChat.Management;
using Microsoft.Rtc.Collaboration.Presence;
using Microsoft.Rtc.Signaling;

namespace PersistentChatLibrarySamples
{
    /// <summary>
    /// Common code that is shared by more than one sample.
    /// </summary>

    internal static class SampleCommon
    {
        #region Customize These To Run Samples In Your Test Environment
        // NOTE: To run the samples in your own test environment, simply change the values in this region.
        public static string LyncServer = "lync.contoso.com";

        // This user must be setup as a Manager of the ROOT Chat Room Category or setup as
        // a "super user" (who can manage every Category/ChatRoom).  To setup a "super user", you
        // can choose them during the Persistent Chat server installation, or you can use the Persistent Chat
        // Admin Tool client to manage that user and check all the boxes under Permissions.
        public static string UserSipUri = "sip:superuser@contoso.com";
        public static string Username = "superuser@contoso.com";
        // Storing passwords in cleartext is an obvious security vulnerability - only 
        // shown here for completeness.
        public static string Password = @"mypassword";
        // If true, this will cause Username and Password to be ignored and will instead use the credentials
        // of the user that is executing this code.  In that case, the UserSipUri is still required to match
        // that authenticating user.
        public static bool UsingSso;

        public static string MemberUserString1 = "sip:member1@contoso.com";
        public static string MemberUserString2 = "sip:member2@contoso.com";
        public static string ManagerUserString = "sip:manager@contoso.com";
        public static string PresenterUserString = "sip:presenter@contoso.com";
        public static Uri ManagerUserUri = new Uri(ManagerUserString);
        public static Uri MemberUserUri1 = new Uri(MemberUserString1);
        public static Uri MemberUserUri2 = new Uri(MemberUserString2);
        public static Uri PresenterUserUri = new Uri(PresenterUserString);

        public static string LoadTestUserUriPrefix = "user";
        public static string LoadTestUserNamePrefix = "user";
        public static string LoadTestDomain = "contoso.com";
        public static string LoadTestLogFile = @"%temp%\LoadTest.log";
        public static int LoadTestFirstUserIndex = 1;
        public static int LoadTestLastUserIndex = 3;

        public static string[] LoadTestChatRoomNames = new[]
            {
                "SampleLoadTestRoom01",
                "SampleLoadTestRoom02",
                "SampleLoadTestRoom03",
                "SampleLoadTestRoom04",
                "SampleLoadTestRoom05",
                "SampleLoadTestRoom06",
                "SampleLoadTestRoom07",
                "SampleLoadTestRoom08",
                "SampleLoadTestRoom09",
                "SampleLoadTestRoom10"
            };
        #endregion

        public static Uri PersistentChatServerUri { get; set; }
 
        public static Uri GetLoadTestUserUri(int index)
        {
            // Change this if the SIP uris of your test users are formatted differently.
            // This assumes SIP uris formatted like: "sip:user1@contoso.com".
            return new Uri(string.Format("sip:{0}{1}@{2}", LoadTestUserUriPrefix, index, LoadTestDomain));
        }

        public static string GetLoadTestUserName(int index)
        {
            // Change this if the user names of your test users are formatted differently.
            // This assumes user names formatted like: "user1@contoso.com" (very similar to the
            // assumed SIP uri format).
            return string.Format("{0}{1}@{2}", LoadTestUserNamePrefix, index, LoadTestDomain);
        }

        public static string GetLoadTestUserPassword(int index)
        {
            // This assumes that test users use passwords same as their names.
            return string.Format("{0}{1}", LoadTestUserNamePrefix, index);
        }

        public static UserEndpoint ConnectLyncServer(string userSipUri, string lyncServer, bool usingSso, string username, string password)
        {
            // Create the Lync Server UserEndpoint and attempt to connect to Lync Server
            Console.WriteLine("Connecting to Lync Server... [{0}]", lyncServer);

            // Use the appropriate SipTransportType depending on current Lync Server deployment
            ClientPlatformSettings platformSettings = new ClientPlatformSettings("PersistentChat.Test", SipTransportType.Tls);
            CollaborationPlatform collabPlatform = new CollaborationPlatform(platformSettings);
            collabPlatform.AllowedAuthenticationProtocol = SipAuthenticationProtocols.Ntlm;

            // Initialize the platform
            collabPlatform.EndStartup(collabPlatform.BeginStartup(null, null));

            // You can also pass in the server's port # here.
            UserEndpointSettings userEndpointSettings = new UserEndpointSettings(userSipUri, lyncServer);

            // When usingSso is true use the current users credentials, otherwise use username and password
            userEndpointSettings.Credential = usingSso ? SipCredentialCache.DefaultCredential : new NetworkCredential(username, password);

            UserEndpoint userEndpoint = new UserEndpoint(collabPlatform, userEndpointSettings);

            // Login to Lync Server.
            userEndpoint.EndEstablish(userEndpoint.BeginEstablish(null, null));

            if (PersistentChatServerUri == null)
            {
                // Extract default Persistent Chat pool uri from inband
                ProvisioningData provisioningData =
                    userEndpoint.EndGetProvisioningData(userEndpoint.BeginGetProvisioningData(null, null));
                PersistentChatServerUri = new Uri(provisioningData.PersistentChatConfiguration.DefaultPersistentChatUri);
            }

            Console.WriteLine("\tSuccess");
            return userEndpoint;
        }

        public static PersistentChatEndpoint ConnectPersistentChatServer(UserEndpoint userEndpoint, Uri persistentChatServerUri)
        {
            Console.WriteLine("Connecting to Persistent Chat Server...");

            PersistentChatEndpoint persistentChatEndpoint = new PersistentChatEndpoint(persistentChatServerUri, userEndpoint);

            persistentChatEndpoint.EndEstablish(persistentChatEndpoint.BeginEstablish(null, null));

            Console.WriteLine("\tSuccess");

            DisplayServerInfo(persistentChatEndpoint.PersistentChatServices.ServerConfiguration);

            return persistentChatEndpoint;
        }

        private static void DisplayServerInfo(PersistentChatServerConfiguration serverConfiguration)
        {
            Console.WriteLine("Server Information:");
            Console.WriteLine("\tDatabase Version: {0}", serverConfiguration.DatabaseVersion);
            Console.WriteLine("\tDisplay Name: {0}", serverConfiguration.DisplayName);
            Console.WriteLine("\tRoom Management URL: {0}", serverConfiguration.RoomManagementUrl);
            Console.WriteLine("\tMessage Size Limit: {0}", serverConfiguration.MessageSizeLimit);
            Console.WriteLine("\tRoot Category URI: {0}", serverConfiguration.RootCategoryUri);
            Console.WriteLine("\tSearch Limit: {0}", serverConfiguration.SearchLimit);
            Console.WriteLine("\tServer Time Utc: {0}", serverConfiguration.ServerTimeUtc.ToShortTimeString());
            Console.WriteLine("\tStory Size Limit: {0}", serverConfiguration.StorySizeLimit);
        }

        public static void DisconnectPersistentChatServer(PersistentChatEndpoint persistentChatEndpoint)
        {
            Console.WriteLine("Disconnecting from Persistent Chat Server...");

            persistentChatEndpoint.EndTerminate(persistentChatEndpoint.BeginTerminate(null, null));

            Console.WriteLine("\tSuccess");
        }

        public static void DisconnectLyncServer(UserEndpoint userEndpoint)
        {
            Console.WriteLine("Disconnecting from Lync Server...");

            userEndpoint.EndTerminate(userEndpoint.BeginTerminate(null, null));

            CollaborationPlatform platform = userEndpoint.Platform;
            platform.EndShutdown(platform.BeginShutdown(null, null));

            Console.WriteLine("\tSuccess");
        }

        public static Uri RoomCreateUnderNonRootCategory(PersistentChatEndpoint persistentChatEndpoint, Uri parentCategoryUri, string chatRoomName)
        {
            Console.WriteLine(String.Format("Create new chat room [{0}] under [{1}]...", chatRoomName, parentCategoryUri));

            ChatRoomManagementServices chatRoomMgmt = persistentChatEndpoint.PersistentChatServices.ChatRoomManagementServices;

            ChatRoomSettings settings = new ChatRoomSettings(chatRoomName, parentCategoryUri);
            Uri roomUri = chatRoomMgmt.EndCreateChatRoom((chatRoomMgmt.BeginCreateChatRoom(settings, null, null)));

            // Add current user as member.            
            PersistentChatUserServices userMgmt = persistentChatEndpoint.PersistentChatServices.UserServices;
            PersistentChatUser user = userMgmt.EndGetUser(userMgmt.BeginGetUser(new Uri(persistentChatEndpoint.InnerEndpoint.OwnerUri), null, null));

            ICollection<PersistentChatPrincipalSummary> members = new List<PersistentChatPrincipalSummary>();
            members.Add(user);
            chatRoomMgmt.EndAddUsersOrGroupsToRole(chatRoomMgmt.BeginAddUsersOrGroupsToRole(ChatRoomRole.Member, roomUri, members, null, null));

            Console.WriteLine("\tSuccess");
            return roomUri;
        }

        public static ChatRoomSnapshot RoomSearchExisting(PersistentChatEndpoint persistentChatEndpoint, string chatRoomName)
        {
            ReadOnlyCollection<ChatRoomSnapshot> chatRooms = RoomSearchWithCriteria(persistentChatEndpoint, chatRoomName);
            return chatRooms != null ? chatRooms[0] : null;
        }

        public static ReadOnlyCollection<ChatRoomSnapshot> RoomSearchWithCriteria(PersistentChatEndpoint persistentChatEndpoint, string chatRoomCriteria, 
                                                                                  int maxResults = 100)
        {
            Console.WriteLine(String.Format("Searching for chat room [{0}] with a maxResults set of [{1}]...", chatRoomCriteria, maxResults));

            PersistentChatServices chatServices = persistentChatEndpoint.PersistentChatServices;

            ReadOnlyCollection<ChatRoomSnapshot> chatRooms = chatServices.EndBrowseChatRoomsByCriteria(
                chatServices.BeginBrowseChatRoomsByCriteria(chatRoomCriteria, false, true, true, maxResults, null, null));

            Console.WriteLine(String.Format("\tFound {0} chat room(s):", chatRooms.Count));
            if (chatRooms.Count > 0)
            {
                foreach (ChatRoomSnapshot snapshot in chatRooms)
                    Console.WriteLine(String.Format("\tname: {0}\n\turi:{1}", snapshot.Name, snapshot.ChatRoomUri));
                return chatRooms;
            }
            return null;
        }

        public static ReadOnlyCollection<ChatRoomSnapshot> RoomSearchUnderCategory(PersistentChatEndpoint persistentChatEndpoint, Uri categoryUri, int maxResults = 100)
        {
            Console.WriteLine(String.Format("Searching for chat rooms under category [{0}] with max result size of [{1}]...", categoryUri, maxResults));

            PersistentChatServices chatServices = persistentChatEndpoint.PersistentChatServices;

            ReadOnlyCollection<ChatRoomSnapshot> chatRooms = chatServices.EndBrowseChatRoomsByCategoryUri(
                chatServices.BeginBrowseChatRoomsByCategoryUri(categoryUri, maxResults, null, null));

            Console.WriteLine(String.Format("\tFound {0} chat room(s):", chatRooms.Count));
            if (chatRooms.Count > 0)
            {
                foreach (ChatRoomSnapshot snapshot in chatRooms)
                    Console.WriteLine(String.Format("\tname: {0}\n\turi:{1}", snapshot.Name, snapshot.ChatRoomUri));
                return chatRooms;
            }
            return null;
        }

        public static ReadOnlyCollection<ChatRoomSnapshot> RoomSearchWithFilterCriteria(PersistentChatEndpoint persistentChatEndpoint, string criteria,
            bool searchDesc, string member, string manager, Uri categoryUri, string addinName, bool disabled, ChatRoomPrivacy? privacy,
            ChatRoomBehavior? behavior, bool? invitations, bool searchInvites, int maxResults)
        {
            Console.WriteLine(string.Format("Searching for chat rooms with max results size of [{0}]...", maxResults));

            PersistentChatServices chatServices = persistentChatEndpoint.PersistentChatServices;
            ChatRoomManagementServices chatRoomManagement = chatServices.ChatRoomManagementServices;

            // get the addin guid if an addin was provided
            Guid? addinGuid = null;
            if (addinName != null)
            {

                ReadOnlyCollection<PersistentChatAddIn> addins =
                    chatRoomManagement.EndGetAllAddIns(chatRoomManagement.BeginGetAllAddIns(null, null));
                foreach (PersistentChatAddIn addin in addins.Where(addin => addin.Name.Equals(addinName)))
                {
                    addinGuid = addin.AddInId;
                    break;
                }
            }

            IAsyncResult asyncResult = chatServices.BeginBrowseChatRoomsByFilterCriteria(criteria, searchDesc,
                                                                                         member, manager,
                                                                                         categoryUri,
                                                                                         addinGuid, disabled, privacy,
                                                                                         behavior, invitations,
                                                                                         searchInvites, maxResults, null,
                                                                                         null);

            ReadOnlyCollection<ChatRoomSnapshot> chatRooms = chatServices.EndBrowseChatRoomsByFilterCriteria(asyncResult);

            Console.WriteLine(String.Format("\tFound {0} chat room(s):", chatRooms.Count));
            if (chatRooms.Count > 0)
            {
                foreach (ChatRoomSnapshot snapshot in chatRooms)
                    Console.WriteLine(String.Format("\tname: {0}\n\turi:{1}", snapshot.Name, snapshot.ChatRoomUri));
                return chatRooms;
            }
            return null;
        }

        public static ChatRoomSession RoomJoinExisting(PersistentChatEndpoint persistentChatEndpoint, ChatRoomSummary summary)
        {
            Console.WriteLine(String.Format("Joining chat room by NAME [{0}]...", summary.Name));

            ChatRoomSession session = new ChatRoomSession(persistentChatEndpoint);
            session.EndJoin(session.BeginJoin(summary, null, null));

            Console.WriteLine("\tSuccess");

            return session;
        }

        public static ChatRoomSession RoomJoinExisting(PersistentChatEndpoint persistentChatEndpoint, Uri roomUri)
        {
            Console.WriteLine(String.Format("Joining chat room by URI [{0}]...", roomUri));

            ChatRoomSession session = new ChatRoomSession(persistentChatEndpoint);
            session.EndJoin(session.BeginJoin(roomUri, null, null));

            Console.WriteLine("\tSuccess");

            return session;
        }

        public static Uri GetCategoryUri(PersistentChatEndpoint persistentChatEndpoint)
        {
            ChatRoomManagementServices chatRoomManagementServices = persistentChatEndpoint.PersistentChatServices.ChatRoomManagementServices;
            IAsyncResult asyncResult = chatRoomManagementServices.BeginFindCategoriesWithCreateRights(null, null);
            ReadOnlyCollection<ChatRoomCategorySummary> categories = chatRoomManagementServices.EndFindCategoriesWithCreateRights(asyncResult);

            int categoryIndex = -1;
            while (categoryIndex < 0 || categoryIndex >= categories.Count)
            {
                Console.WriteLine(string.Format(
                        "Please enter the index of the category you would like to work under (0-based): [{0}]",
                        string.Join(", ", categories.Select(cat => cat.Name))));
                int.TryParse(Console.ReadLine(), out categoryIndex);
            }
            return categories[categoryIndex].Uri;
        }
    }
}
