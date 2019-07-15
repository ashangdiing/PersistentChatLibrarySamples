// Copyright (c) Microsoft Corporation.  All rights reserved. 


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.PersistentChat;
using SampleChat;

namespace PersistentChatLibrarySamples
{
    /// <summary>
    /// Sample that demonstrates how to create and join a Chat Room and then post chat messages to that
    /// Chat Room.
    /// </summary>
    static class SampleChat
    {
        private static readonly string TestRunUniqueId = Guid.NewGuid().ToString();
        
        public static void Main(string[] args)
        {
            ChatClient mainClient,cc1,cc2;
           
            mainClient = new ChatClient("test2", "esun#5005");
            string rommName = "aaaabbbb :" + TestRunUniqueId;
            rommName = "ITChatRoom1";
            cc1 = new ChatClient("test1", "esun#5005");
            cc2 = new ChatClient("test3", "esun#5005");
            mainClient.loginLyncServer();
           // mainClient.creatRoom(rommName);
            Console.WriteLine(@" cSetChatRoomPrivacySetting -->uri: " + mainClient.roomUri);
         //   mainClient.SetChatRoomPrivacySetting(mainClient.persistentChatEndpoint, mainClient.roomUri, ChatRoomPrivacy.Closed);
            Console.WriteLine(@" cSetChatRoomPrivacySetting----------------> ");
             mainClient.RoomLeave(mainClient.roomSession);

        //   cc1.loginLyncServer();
        //    cc2.loginLyncServer();
          mainClient.loginLyncServer();


            Console.ReadLine();
            mainClient.enterRoom(rommName);
            mainClient.RoomChatHistory(mainClient.roomSession, 1000);
            mainClient.RoomSearchChatHistory(mainClient.persistentChatEndpoint, mainClient.roomSession, "story body");
          //  cc1.enterRoom(rommName);
          //  cc2.enterRoom(rommName);
            Console.ReadKey();
         //   cc1.loginLyncServer();

           // cc1.enterRoom(mainClient.roomUri);

          //  cc1.enterRoom(rommName);
          mainClient.ChatRoomAddManagerAndMember(mainClient.persistentChatEndpoint, mainClient.roomUri, cc1.sipUri);

         //   mainClient.ChatRoomAddManagerAndMember(cc1.persistentChatEndpoint, mainClient.persistentChatEndpoint, mainClient.roomUri, cc1.sipUri);
            Console.Write(@" creat romm  leave ");
            Console.ReadLine();

         //  cc1.RoomLeave(cc1.roomSession);
         //   cc2.RoomLeave(cc2.roomSession);
            mainClient.RoomLeave(mainClient.roomSession);
        }

        public static void startSampleChat()
        {
         string chatRoomName = "A5 ngwei---1" + TestRunUniqueId;

            try
            {
                ChatRoomSession roomSession;

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

                // Create a new chat room
                Uri roomUri = SampleCommon.RoomCreateUnderNonRootCategory(persistentChatEndpoint, catUri, chatRoomName);










                // Change this to try out different ways to join the same channel
                string answer = GetRoomAnswer();

                bool joinByUri = answer.Equals("U");
                if (joinByUri)
                {
                    // OPTION 1: Join by using the result of the Create operation:
                    roomSession = SampleCommon.RoomJoinExisting(persistentChatEndpoint, roomUri);
                }
                else
                {
                    // OPTION 2: Join by searching for the room by name:
                    ChatRoomSnapshot roomSnapshot = SampleCommon.RoomSearchExisting(persistentChatEndpoint, chatRoomName);
                    roomSession = SampleCommon.RoomJoinExisting(persistentChatEndpoint, roomSnapshot);
                }

                // Chat in the chat room
                RoomChat(roomSession);

                // Get the chat history from the room
                RoomChatHistory(roomSession);

                // Search the chat history for a room
                RoomSearchChatHistory(persistentChatEndpoint, roomSession, "story body");
                SampleLoadTest.roomUri = roomUri;
              //  SampleLoadTest.getClient();
                
              //  SampleLoadTest.stopClient();


                Console.Write(@" roomSession--------   leave ");
                 Console.ReadLine();


                // Leave room
                RoomLeave(roomSession);


                Console.Write(@" DisconnectPersistentChatServer   leave ");
                Console.ReadLine();
                // Disconnect from Persistent Chat and from Lync Server
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


        private static string GetRoomAnswer()
        {
            while (true)
            {
                Console.Write(@"Join room by uri or searching (U/S): ");
                string answer = Console.ReadLine();
                answer = (answer != null ? answer.ToUpper() : null);
                if (answer != null && (answer.Equals("U") || answer.Equals("S")))
                    return answer;
            }
        }

        private static void RoomChat(ChatRoomSession session)
        {
            Console.WriteLine(String.Format("Chatting in chat room [{0}]...", session.Name));

            Console.WriteLine("\tsubscribe to incoming messages");
            session.ChatMessageReceived += SessionChatMessageReceived;

            // Send a simple chat message
            const string rtfContent = @"{\rtf1\ansi\f0\pard This is a simple message with {\b RTF}\par}";
            session.EndSendChatMessage(session.BeginSendChatMessage("This is a simple message with RTF", rtfContent, null, null));

            // Send a simple story
            FormattedOutboundChatMessage chatSimpleStory = new FormattedOutboundChatMessage(false, "story title");
            chatSimpleStory.AppendPlainText("story body");
            session.EndSendChatMessage(session.BeginSendChatMessage(chatSimpleStory, null, null));

            // Send a more complicated message
            FormattedOutboundChatMessage chatAdvancedMessage = new FormattedOutboundChatMessage(true);
            chatAdvancedMessage.AppendPlainText("This alert message has a channel link: ");
            chatAdvancedMessage.AppendChatRoomLink(session);
            chatAdvancedMessage.AppendPlainText(" it also has a hyperlink: ");
            chatAdvancedMessage.AppendHyperLink("skypetest.com", new Uri("http://sfb2015.skypetest.com"));
            session.EndSendChatMessage(session.BeginSendChatMessage(chatAdvancedMessage, null, null));

            session.ChatMessageReceived -= SessionChatMessageReceived;
            Console.WriteLine("\tSuccess");
        }

        private static void RoomChatHistory(ChatRoomSession session)
        {
            Console.WriteLine(String.Format("Obtaining the chat history in chat room [{0}]...", session.Name));

            // Get the three messages that were sent before, the simple chat message, the simple story, and the complicated message
            ReadOnlyCollection<ChatMessage> chatMessages = session.EndGetRecentChatHistory(session.BeginGetRecentChatHistory(3, null, null));

            Console.WriteLine(string.Format("Three most recent chat history messages: \n\t1) {0} \n\t2) {1} \n\t3) {2}", 
                                            chatMessages[0].MessageContent, chatMessages[1].MessageContent, chatMessages[2].MessageContent));
        }

        private static void RoomSearchChatHistory(PersistentChatEndpoint persistentChatEndpoint, ChatRoomSession session, string searchString)
        {
            Console.WriteLine(string.Format("Searching the chat history in chat room [{0}] for string [{1}]", session.Name, searchString));
            
            // Search the chat history
            IAsyncResult asyncResult = persistentChatEndpoint.PersistentChatServices.BeginQueryChatHistory(new List<Uri> { session.ChatRoomUri },
                                                                      searchString, false, false, null, null);
            ReadOnlyCollection<ChatHistoryResult> results = persistentChatEndpoint.PersistentChatServices.EndQueryChatHistory(asyncResult);

            foreach (ChatHistoryResult result in results)
            {
                Console.WriteLine(string.Format("\tChat Room [{0}] contains the following matches:", result.ChatRoomName));
                foreach (ChatMessage message in result.Messages)
                {
                    Console.WriteLine(string.Format("\t\tMessage: {0}", message.MessageContent));
                }
            }
        }

        private static void RoomLeave(ChatRoomSession session)
        {
            Console.WriteLine(String.Format("Leaving chat room [{0}]...", session.Name));

            session.EndLeave(session.BeginLeave(null, null));

            Console.WriteLine("\tSuccess");
        }

        static void SessionChatMessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            Console.WriteLine("\tChat message received");
            Console.WriteLine(String.Format("\t from:[{0}]", e.Message.MessageAuthor));
            Console.WriteLine(String.Format("\t room:[{0}]", e.Message.ChatRoomName));
            Console.WriteLine(String.Format("\t body:[{0}]", e.Message.MessageContent));

            if (e.Message.MessageRtfContent != null)
                Console.WriteLine(String.Format("\t rtf:[{0}]", e.Message.MessageRtfContent));

            foreach (MessagePart part in e.Message.FormattedMessageParts)
                Console.WriteLine(String.Format("\t part:[{0}]", part.RawText));
        }

       











      


















    }
}
