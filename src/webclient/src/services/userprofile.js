import { userIsLoggedIn, userProfile } from "../store";
import auth from "./authentication.js";
import helper from "./helper.js";

class UserProfile {
    constructor() {
        this.init();
    }
    async init() {
        const currentUser = await auth.getUser();
        if (currentUser) {
            console.log(currentUser);
            userIsLoggedIn.set(true);
        } else {
            console.log("not logged in");
        }
        userIsLoggedIn.subscribe(async value => {
            if (value) {
                this.setLoggedInUserProfile();
            } else {
                this.setLoggedOutUserProfile();
            }
        });
        auth.onUserLoaded((u) => this.onuserIsLoggedIn(u));
    }
    
    onuserIsLoggedIn(loggedInUser) {
        console.log("onuserIsLoggedIn");
        if (loggedInUser) {
            userIsLoggedIn.set(true);
            this.setLoggedInUserProfile(loggedInUser);
        }
    }
    async setLoggedInUserProfile(user) {
        if (!user)
            user = await auth.getUser();
        
        if (!user)
            return;
        
        const email = user.profile.email;
        const name = user.profile.name;
        const role = user.profile.role;
        const up = {
            loggedIn: true,
            id_token: user.id_token,
            access_token: user.access_token,
            session_state: user.session_state,
            refresh_token: user.refresh_token,
            token_type: user.token_type,
            expires_at: user.expires_at,
            scope: user.scope,
            idp: user.profile.idp,
            sid: user.profile.sid,
            sub: user.profile.sub,
            name,
            email,
            role,
            initials: helper.getInitials(email)
        }
        userProfile.set(up);
    }
    setLoggedOutUserProfile() {
        userProfile.set({ loggedIn: false, name: "anon", initials: "U" });
    }
}
export default new UserProfile();
