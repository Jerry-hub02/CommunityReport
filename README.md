This is a website I built in 2025 for my research project, where people in a community can report problems in their area, like potholes or sewage issues. 
Instead of calling someone or going to a government office, residents can simply log in and submit their complaint online with a description, address, 
and even a photo of the problem.
Once a complaint is submitted, the admin can see it, change its status to show whether it's being worked on or has been resolved, 
and send the user a notification to keep them updated. This way, people always know what is happening with their complaint without having to follow up themselves.
The website also has a chat room feature where community members can talk to each other in real time. Users can create chat rooms, join existing ones, 
and send messages instantly without refreshing the page, which I used SignalR.

On top of that, the admin can send notifications to a specific user or broadcast a message to everyone at once. 
Users can see these notifications through a bell icon at the top of the page, which shows how many unread messages they have.
Each user has their own profile where they can update their username, email, and password. 
There is also a personal dashboard that gives users a quick summary of their complaints, notifications, and chat rooms all in one place.
The system has two types of users, regular users and admins, and each sees a different version of the website based on their role. 
Regular users manage their own complaints while admins manage everything.

Overall, this project makes it easier for communities to report problems and for authorities to respond to them in an organized and transparent way.

Notite: Admin can only be registered inside a code, here is the path- CommunityReport\Controllers\UserAuthentication.cs
After writing your own credintials and start to deburg. Start first with this link to register admin: https://localhost:7043/UserAuthentication/Reg
