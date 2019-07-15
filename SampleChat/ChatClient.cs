using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.PersistentChat;
using Microsoft.Rtc.Collaboration.PersistentChat.Management;
using PersistentChatLibrarySamples;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SampleChat
{
    public class ChatClient
    {
        public static string domain = "skypetest.com";
        public string userName;
        public string userDomanName;
        public string password;
        public PersistentChatEndpoint persistentChatEndpoint;
        public UserEndpoint userEndpoint;
        public Uri sipUri;
        public Uri categoryUri;
        public ChatRoomSession roomSession;
        public Uri roomUri;

        public ChatClient(string userNmae, string password)
        {
            this.userName = userNmae;
            this.userDomanName = userNmae + "@" + domain;
            this.sipUri = new Uri("sip:" + this.userDomanName);
            this.password = password;
            Console.WriteLine(String.Format(" userNmae   [{0}]...", userNmae));
            Console.WriteLine(String.Format(" userDomanName   [{0}]...", userDomanName));
            Console.WriteLine(String.Format(" sipUri   [{0}]...", sipUri));
            Console.WriteLine(String.Format(" password   [{0}]...", password));
        }


        public void loginLyncServer()
        {
            // Connect to Lync Server
            userEndpoint = SampleCommon.ConnectLyncServer(sipUri.ToString(),
                                                                      SampleCommon.LyncServer,
                                                                      SampleCommon.UsingSso,
                                                                      userDomanName,
                                                                      password);

            // Connect to Persistent Chat Server
            persistentChatEndpoint = SampleCommon.ConnectPersistentChatServer(userEndpoint, SampleCommon.PersistentChatServerUri);
            Console.WriteLine(String.Format("please press any key continue login success    [{0}]...", userName));
            Console.ReadLine();
        }


        public ChatRoomSession creatRoom(String chatRoomName)
        {

            // Get a category
            categoryUri = SampleCommon.GetCategoryUri(persistentChatEndpoint, userName);

            Console.WriteLine(String.Format(" creatRoom name   [{0}]...", chatRoomName));

            // Create a new chat room
            roomUri = SampleCommon.RoomCreateUnderNonRootCategory(persistentChatEndpoint, categoryUri, chatRoomName);

            roomSession = SampleCommon.RoomJoinExisting(persistentChatEndpoint, roomUri);

            //  roomSession = new ChatRoomSession(persistentChatEndpoint);  这里只能join，这种会提示状态不对，可能需要等几秒才正常
            // Chat in the chat room
            RoomChat(roomSession);

            // Get the chat history from the room
            RoomChatHistory(roomSession,3);

            // Search the chat history for a room
            RoomSearchChatHistory(persistentChatEndpoint, roomSession, "story body");
            Console.WriteLine(String.Format(" creatRoom name   [{0}] ------> info", chatRoomName));

            return roomSession;
        }

        public ChatRoomSession enterRoom(String chatRoomName)
        {

            ChatRoomSnapshot roomSnapshot = SampleCommon.RoomSearchExisting(persistentChatEndpoint, chatRoomName);
            roomSession = SampleCommon.RoomJoinExisting(persistentChatEndpoint, roomSnapshot);
            Console.WriteLine(String.Format(" enterRoom name   [{0}] ------> state:{1}", chatRoomName, roomSession.State));
            foreach (ChatRoomParticipant cp in roomSession.Participants)
            {
                Console.WriteLine(String.Format(" enterRoom ChatRoomParticipant name   [{0}] ------> info:{1}  isManger:{2}", chatRoomName,cp.SipUri,cp.IsManager));
               
            }

            Console.WriteLine(String.Format(" enterRoom ChatRoomParticipant name  is out room [{0}]  total :{1}", chatRoomName, roomSession.Participants.Count));
            

            return roomSession;
        }

        public ChatRoomSession enterRoom(Uri url)
        {
            roomSession = SampleCommon.RoomJoinExisting(persistentChatEndpoint, roomUri);
            return roomSession;
        }



        public void ChatRoomAddManagerAndMember(PersistentChatEndpoint persistentChatEndpoint, Uri chatroomUri, Uri userUri)
        {
            Console.WriteLine(String.Format("Adding manager+member [{0}] to Room [{1}]...", userUri, chatroomUri));

            ChatRoomManagementServices chatroomMgmt = persistentChatEndpoint.PersistentChatServices.ChatRoomManagementServices;
            PersistentChatUserServices userMgmt = persistentChatEndpoint.PersistentChatServices.UserServices;

            PersistentChatUser user = userMgmt.EndGetUser(userMgmt.BeginGetUser(userUri, null, null));
            List<PersistentChatPrincipalSummary> newUsers = new List<PersistentChatPrincipalSummary> { user };

            chatroomMgmt.EndAddUsersOrGroupsToRole((chatroomMgmt.BeginAddUsersOrGroupsToRole(ChatRoomRole.Manager, chatroomUri, newUsers, null, null)));
        //    chatroomMgmt.EndAddUsersOrGroupsToRole((chatroomMgmt.BeginAddUsersOrGroupsToRole(ChatRoomRole.Member, chatroomUri, newUsers, null, null)));

            Console.WriteLine("\t {0} client {1} ChatRoomAddManagerAndMember  {2} Success", userName, chatroomUri, userUri);
        }


        public void ChatRoomAddManagerAndMember(PersistentChatEndpoint persistentChatEndpoint, PersistentChatEndpoint targetPersistentChatEndpoint, Uri chatroomUri, Uri userUri)
        {
            Console.WriteLine(String.Format("Adding manager+member [{0}] to Room [{1}]...", userUri, chatroomUri));

            ChatRoomManagementServices chatroomMgmt = persistentChatEndpoint.PersistentChatServices.ChatRoomManagementServices;
            PersistentChatUserServices userMgmt = persistentChatEndpoint.PersistentChatServices.UserServices;

            PersistentChatUser user = userMgmt.EndGetUser(userMgmt.BeginGetUser(userUri, null, null));
            if (user != null)
            {
                Console.WriteLine(String.Format("Adding manager user is mull member [{0}] to Room [{1}]...", userUri, chatroomUri));
            }
            List<PersistentChatPrincipalSummary> newUsers = new List<PersistentChatPrincipalSummary> { user };

            ChatRoomManagementServices targetChatroomMgmt = targetPersistentChatEndpoint.PersistentChatServices.ChatRoomManagementServices;

            //   targetChatroomMgmt.EndAddUsersOrGroupsToRole((chatroomMgmt.BeginAddUsersOrGroupsToRole(ChatRoomRole.Manager, chatroomUri, newUsers, null, null)));
            targetChatroomMgmt.EndAddUsersOrGroupsToRole((chatroomMgmt.BeginAddUsersOrGroupsToRole(ChatRoomRole.Member, chatroomUri, newUsers, null, null)));

            Console.WriteLine("\t {0} client {1} ChatRoomAddManagerAndMember  {2} Success", userName, chatroomUri, userUri);
        }















































        public void SetChatRoomPrivacySetting(PersistentChatEndpoint persistentChatEndpoint, Uri roomUri, ChatRoomPrivacy privacySetting)
        {


            Console.WriteLine("\t {0} client {1} SetChatRoomPrivacySetting privacySetting {2}  start", userName, roomUri, privacySetting);
            ChatRoomManagementServices ChatRoomServices = persistentChatEndpoint.PersistentChatServices.ChatRoomManagementServices;

            ChatRoom room = ChatRoomServices.EndGetChatRoom(
                ChatRoomServices.BeginGetChatRoom(roomUri, null, null));
            ChatRoomInformation roomInfo = new ChatRoomInformation(room);
            roomInfo.Privacy = privacySetting;
            ChatRoomServices.EndUpdateChatRoom(
                ChatRoomServices.BeginUpdateChatRoom(roomInfo, null, null));
            Console.WriteLine("\t {0} client {1} SetChatRoomPrivacySetting privacySetting {2} Success", userName, roomUri, privacySetting);
        }











        public void RoomChat(ChatRoomSession session)
        {
            if (session == null)
                return;

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

        public void RoomChatHistory(ChatRoomSession session,int recentCount)
        {
            if (session == null)
                return;

            Console.WriteLine(String.Format("Obtaining the chat history in chat room [{0}]. 最近{1}条", session.Name, recentCount));

            // Get the three messages that were sent before, the simple chat message, the simple story, and the complicated message
            ReadOnlyCollection<ChatMessage> chatMessages = session.EndGetRecentChatHistory(session.BeginGetRecentChatHistory(recentCount, null, null));
            for (int i = 0; i < chatMessages.Count; i++) {
                Console.WriteLine(string.Format("recent chat history messages \n\t 第{0}条:ID:{1} 作者：{2} 时间: {3}内容：{4}", i, chatMessages[i].MessageId, chatMessages[i].MessageAuthor, chatMessages[i].Timestamp, chatMessages[i].MessageContent));
            }

        }

        public void RoomSearchChatHistory(PersistentChatEndpoint persistentChatEndpoint, ChatRoomSession session, string searchString)
        {
            if (session == null)
                return;
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

        public void RoomLeave(ChatRoomSession session)
        {
            if (session == null)
                return;

            Console.WriteLine(String.Format("Leaving chat room [{0}]...{1}", session.Name, session.State));

            session.EndLeave(session.BeginLeave(null, null));

            Console.WriteLine("\tSuccess");
        }

        public void SessionChatMessageReceived(object sender, ChatMessageReceivedEventArgs e)
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
