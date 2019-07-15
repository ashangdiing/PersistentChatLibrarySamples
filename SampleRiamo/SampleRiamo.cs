// Copyright (c) Microsoft Corporation.  All rights reserved. 


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.PersistentChat;
using Microsoft.Rtc.Collaboration.PersistentChat.Management;

namespace PersistentChatLibrarySamples
{
    /// <summary>
    /// Sample that demonstrates how to retrieve the list of rooms of which the user is a member and those
    /// that the user can manage.
    /// </summary>
    static class SampleRiamo
    {
        private static readonly string TestRunUniqueId = Guid.NewGuid().ToString();

        public static void Main(string[] args)
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////
            // Note: Assuming that category(ies) have been created and this user is a creator on some of them
            //////////////////////////////////////////////////////////////////////////////////////////////////

            try
            {
                // Connect to Lync Server
                UserEndpoint userEndpoint = SampleCommon.ConnectLyncServer(SampleCommon.UserSipUri,
                                                                           SampleCommon.LyncServer,
                                                                           SampleCommon.UsingSso,
                                                                           SampleCommon.Username,
                                                                           SampleCommon.Password);

                // Connect to Persistent Chat Server
                PersistentChatEndpoint persistentChatEndpoint = SampleCommon.ConnectPersistentChatServer(userEndpoint,
                                                                                          SampleCommon.
                                                                                              PersistentChatServerUri);

                SetupRooms(persistentChatEndpoint);

                GetRiamoLists(persistentChatEndpoint);

                // Disconnect from both Persistent Chat Server and Lync Server
                SampleCommon.DisconnectPersistentChatServer(persistentChatEndpoint);
                SampleCommon.DisconnectLyncServer(userEndpoint);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                Console.Out.WriteLine("InvalidOperationException: " + invalidOperationException.Message);
            }
            catch (ArgumentNullException argumentNullException)
            {
                Console.Out.WriteLine("ArgumentNullException: " + argumentNullException.Message);
            }
            catch (ArgumentException argumentException)
            {
                Console.Out.WriteLine("ArgumentException: " + argumentException.Message);
            }
            catch (Microsoft.Rtc.Signaling.AuthenticationException authenticationException)
            {
                Console.Out.WriteLine("AuthenticationException: " + authenticationException.Message);
            }
            catch (Microsoft.Rtc.Signaling.FailureResponseException failureResponseException)
            {
                Console.Out.WriteLine("FailureResponseException: " + failureResponseException.Message);
            }
            catch (UriFormatException uriFormatException)
            {
                Console.Out.WriteLine("UriFormatException: " + uriFormatException.Message);
            }
            catch (Exception exception)
            {
                Console.Out.WriteLine("Exception: " + exception.Message);
            }
        }

        private static void SetupRooms(PersistentChatEndpoint persistentChatEndpoint)
        {
            string chatRoomName = "SampleRiamo_Room" + TestRunUniqueId;

            // Get a category
            Uri catUri = SampleCommon.GetCategoryUri(persistentChatEndpoint);

            // Create a new chat room
            Uri roomUri = SampleCommon.RoomCreateUnderNonRootCategory(persistentChatEndpoint, catUri, chatRoomName);

            PersistentChatUserServices userMgmt =
                persistentChatEndpoint.PersistentChatServices.UserServices;

            PersistentChatUser persistentChatUser = userMgmt.EndGetUser(userMgmt.BeginGetUser(new Uri(persistentChatEndpoint.InnerEndpoint.OwnerUri), null, null));

            ChatRoomManagementServices chatRoomMgmt = persistentChatEndpoint.PersistentChatServices.ChatRoomManagementServices;

            try
            {
                ICollection<PersistentChatPrincipalSummary> members = new List<PersistentChatPrincipalSummary> { persistentChatUser };
                chatRoomMgmt.EndAddUsersOrGroupsToRole(chatRoomMgmt.BeginAddUsersOrGroupsToRole(ChatRoomRole.Member,
                                                                                                roomUri, members, null,
                                                                                                null));
            }
            catch (CommandFailedException cfe)
            {
                if (!cfe.ToString().Contains("principals are already defined"))
                    throw;
            }
        }

        private static void GetRiamoLists(PersistentChatEndpoint persistentChatEndpoint)
        {
            PersistentChatServices pcs = persistentChatEndpoint.PersistentChatServices;

            ReadOnlyCollection<ChatRoomSnapshot> chatRooms = pcs.EndBrowseMyChatRooms(pcs.BeginBrowseMyChatRooms(null, null, 1000));
            Console.WriteLine("My Rooms:");
            DumpChatRooms(chatRooms);

            chatRooms = pcs.EndBrowseChatRoomsIManage(pcs.BeginBrowseChatRoomsIManage(null, null, 1000));
            Console.WriteLine("Rooms that I manage:");
            DumpChatRooms(chatRooms);
        }

        private static void DumpChatRooms(IEnumerable<ChatRoomSnapshot> chatRooms)
        {
            foreach (ChatRoomSnapshot chatRoom in chatRooms)
            {
                Console.WriteLine(String.Format("\t Uri: [{0}]\n", chatRoom.ChatRoomUri));
                Console.WriteLine(String.Format("\t Name:[{0}]", chatRoom.Name));
                Console.WriteLine(String.Format("\t Desc:[{0}]", chatRoom.Description));
            }
        }
    }
}
