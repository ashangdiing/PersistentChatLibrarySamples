// Copyright (c) Microsoft Corporation.  All rights reserved. 
//
// WARNING: This sample should not be executed against a production system since it will create 
//          categories and chat rooms that cannot be deleted (only disabled) due to the persistent 
//          nature of Persistent Chat. 

namespace PersistentChatLibrarySamples
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Rtc.Collaboration;
    using Microsoft.Rtc.Collaboration.PersistentChat;

    static class SampleSearchRooms
    {
        private static readonly string TestRunUniqueId = Guid.NewGuid().ToString();
        private static readonly Random Random = new Random();

        public static void Main(string[] args)
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////
            // Note: Assuming that category(ies) have been created and this user is a creator on some of them
            /////////////////////////////////////////////////////////////////////////////////////////////////

            try
            {
                // Connect to Lync Server
                UserEndpoint userEndpoint = SampleCommon.ConnectLyncServer(SampleCommon.UserSipUri,
                                                                           SampleCommon.LyncServer,
                                                                           SampleCommon.UsingSso,
                                                                           SampleCommon.Username,
                                                                           SampleCommon.Password);

                // Connect to Persistent Chat Server
                PersistentChatEndpoint persistentChatEndpoint = SampleCommon.ConnectPersistentChatServer(userEndpoint, SampleCommon.PersistentChatServerUri);

                // Get a category
                Uri catUri = SampleCommon.GetCategoryUri(persistentChatEndpoint);

                // create 10 chat rooms
                IList<ChatRoomHelper> chatRooms = new List<ChatRoomHelper>();
                for (int i = 0; i < 10; i++)
                {
                    // Create a new chat room
                    string chatRoomName;
                    if (i < 5)
                        chatRoomName = "SampleSearch_TestRoom" + TestRunUniqueId;
                    else
                        chatRoomName = "Sample_TestRoom" + TestRunUniqueId;
                    chatRoomName += i;
                    Uri chatRoomUri = SampleCommon.RoomCreateUnderNonRootCategory(persistentChatEndpoint, catUri, chatRoomName);
                    chatRooms.Add(new ChatRoomHelper(chatRoomName, chatRoomUri));
                }

                // Find chat rooms by name
                int randIndex = Random.Next(10);
                BrowseChatRoomByName(persistentChatEndpoint, chatRooms[randIndex].ChatRoomName);

                // Find chat rooms under a category
                BrowseChatRoomByCategory(persistentChatEndpoint, catUri);

                // Find chat rooms by criteria
                BrowseChatRoomsByCriteria(persistentChatEndpoint, "Sample_TestRoom");

                // Find chat rooms by filter conditions:
                //  Search Criteria: Name has "Sample" in it, UserSipUri is a member of the room, The room is Scoped and of type
                //                   Normal, and has invitations settings set to Inherit and we want only 100 results
                //  All search criteria that we don't want to search under is passed as null
                BrowseChatRoomsByFilterCriteria(persistentChatEndpoint, "Sample", false, SampleCommon.UserSipUri, null, null, null, false,
                    ChatRoomPrivacy.Closed, ChatRoomBehavior.Normal, null, true, 100);

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

        private static void BrowseChatRoomByName(PersistentChatEndpoint persistentChatEndpoint, string chatRoomName)
        {
            Console.WriteLine(string.Format("Searching for chat room with name = [{0}]", chatRoomName));
            ChatRoomSnapshot chatRoom = SampleCommon.RoomSearchExisting(persistentChatEndpoint, chatRoomName);

            Console.WriteLine(string.Format("\tFound chat room: [Name: {0}, Description: {1}, NumParticipants: {2}, Uri: {3}]", 
                                    chatRoom.Name, chatRoom.Description, chatRoom.NumberOfParticipants, chatRoom.ChatRoomUri));
        }

        private static void BrowseChatRoomByCategory(PersistentChatEndpoint persistentChatEndpoint, Uri categoryUri)
        {
            Console.WriteLine(string.Format("Searching for chat rooms under category [{0}]", categoryUri));

            SampleCommon.RoomSearchUnderCategory(persistentChatEndpoint, categoryUri);
        }

        private static void BrowseChatRoomsByCriteria(PersistentChatEndpoint persistentChatEndpoint, string criteria)
        {
            Console.WriteLine(string.Format("Searching for chat rooms with criteria [{0}]", criteria));

            SampleCommon.RoomSearchWithCriteria(persistentChatEndpoint, criteria);
        }

        private static void BrowseChatRoomsByFilterCriteria(PersistentChatEndpoint persistentChatEndpoint, string criteria, bool searchDesc,
            string member, string manager, Uri categoryUri, string addinName, bool disabled, ChatRoomPrivacy? privacy,
            ChatRoomBehavior? behavior, bool? invites, bool searchInvites, int maxResults)
        {
            Console.WriteLine(string.Format("Searchinf for chat rooms satisfying filter conditions"));

            SampleCommon.RoomSearchWithFilterCriteria(persistentChatEndpoint, criteria, searchDesc, member, manager,
                                                      categoryUri, addinName, disabled, privacy, behavior, 
                                                      invites, searchInvites, maxResults);
        }

        private class ChatRoomHelper
        {
            public ChatRoomHelper(string chatRoomName, Uri chatRoomUri)
            {
                ChatRoomName = chatRoomName;
                ChatRoomUri = chatRoomUri;
            }

            public string ChatRoomName { get; private set; }
            public Uri ChatRoomUri { get; private set; }
        }
    }
}
