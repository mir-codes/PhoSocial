
<div align="center">
	<img src="https://img.icons8.com/ios-filled/100/000000/camera.png" width="60" />
	<h1 style="font-size:2.5rem; color:#222; margin-bottom:0;">PhoSocial</h1>
	<p style="font-size:1.2rem; color:#888;">A modern, full-stack social media platform for sharing photos, chatting, and connecting with friends.</p>
</div>

---

## 🚀 What is PhoSocial?

PhoSocial is a stylish, responsive social media app built with Angular and .NET Core WebAPI. It lets users:

- 📸 Share photos and posts
- 💬 Chat in real-time
- 🔍 Search for users
- 👍 Like and comment on posts
- 👤 Manage their profile

All features are designed for a beautiful, user-friendly experience on any device.

---

## 🗂️ Folder Structure & File Guide

```
PhoSocial/
├── PhoSocialService/
│   ├── PhoSocial.API/         # .NET Core WebAPI backend
│   │   ├── Controllers/       # API endpoints (Auth, Feed, Chat, Search, etc.)
│   │   ├── DTOs/              # Data Transfer Objects for requests/responses
│   │   ├── Hubs/              # SignalR hubs for real-time chat
│   │   ├── Models/            # Entity models (User, Post, Comment, etc.)
│   │   ├── Repositories/      # Data access logic
│   │   ├── Services/          # Business logic (Auth, Chat, etc.)
│   │   ├── Utilities/         # Helpers (JWT, etc.)
│   │   ├── appsettings.json   # API configuration
│   │   └── ...
│   └── PhoSocial.UI/          # Angular frontend
│       ├── src/
│       │   ├── app/
│       │   │   ├── pages/     # Main pages (feed, chat, profile, login, signup, post detail, user search)
│       │   │   ├── services/  # Angular services for API calls (auth, feed, chat, search)
│       │   │   ├── shared/    # Reusable UI components (topbar, footer, searchbar, message list, etc.)
│       │   │   ├── interceptors/ # HTTP interceptors (auth token)
│       │   │   ├── app.module.ts  # Main Angular module
│       │   │   ├── app-routing.module.ts # Routing config
│       │   │   └── ...
│       │   ├── assets/        # Static assets (images, icons)
│       │   ├── environments/  # Environment configs
│       │   ├── styles.scss    # Global styles (Bootstrap, custom)
│       │   └── ...
│       ├── angular.json       # Angular project config
│       ├── package.json       # NPM dependencies
│       └── ...
└── README.md                  # This file
```

---

## 🌟 Features

- **User Authentication:** Secure signup/login with JWT
- **Feed:** View, like, and comment on posts
- **Photo Upload:** Create posts with images
- **Profile:** View and edit user info
- **User Search:** Find users by name or email
- **Chat:** Real-time messaging using SignalR
- **Responsive Design:** Works beautifully on desktop and mobile
- **Modern UI:** Clean, light, and friendly interface

---

## 🛠️ How to Run

1. **Backend:**
	 - Go to `PhoSocialService/PhoSocial.API`
	 - Run: `dotnet run`
2. **Frontend:**
	 - Go to `PhoSocialService/PhoSocial.UI`
	 - Run: `npm install` then `ng serve`
3. Open [http://localhost:4200](http://localhost:4200) in your browser

---

## 💡 Contributing & Customization

- Fork, clone, and explore the code!
- All main features are modular and easy to extend.
- UI is built with Bootstrap and custom styles for easy theming.

---

## 📬 Contact & Credits

- Created by [mir-codes](https://github.com/mir-codes)
- Icons by [Icons8](https://icons8.com)

---

<div align="center" style="margin-top:2rem;">
	<img src="https://img.icons8.com/ios-filled/50/000000/like.png" width="40" />
	<img src="https://img.icons8.com/ios-filled/50/000000/comments.png" width="40" />
	<img src="https://img.icons8.com/ios-filled/50/000000/user-group-man-man.png" width="40" />
	<img src="https://img.icons8.com/ios-filled/50/000000/camera.png" width="40" />
	<br/>
	<span style="color:#888; font-size:1.1rem;">Enjoy building and sharing with PhoSocial!</span>
</div>
