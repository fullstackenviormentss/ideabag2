<template>
    <div class="appContainer">
        <div class="form-group ">
            <label for="emailTb ">Email address</label>
            <input v-model="formData.email" id="emailTb" type="email" class="form-control " placeholder="Enter email ">
        </div>
        <div class="form-group ">
            <label for="passwordTb ">Password</label>
            <input v-model="formData.password" type="password" class="form-control " id="passwordTb " placeholder="Password ">
        </div>
        <button @click="loginUser" class="appBtn" :disabled="this.$store.getters.isPerformingAction">Login</button>

        <img v-if="this.$store.getters.isPerformingAction" id="loadingCircle" src="https://samherbert.net/svg-loaders/svg-loaders/oval.svg" />
    </div>
</template>

<script>
import eventbus from '../eventbus';

export default {
	data() {
		return {
			formData: {
				email: '',
				password: ''
			},
			errorCodes: {
				INVALID_PASSWORD: 'A user with that email and password was not found.',
				EMAIL_NOT_FOUND: 'A user with that email address was not found.',
				USER_DISABLED:
					'The user account has been disabled by an administrator.',
				TOO_MANY_ATTEMPTS:
					'Too many login attempts. Please try again in a few minutes.'
			}
		};
	},
	methods: {
		loginUser() {
			if (this.formData.email.length > 0 && this.formData.password.length > 0) {
				this.$store.dispatch('loginUser', this.formData);
			} else {
				eventbus.showToast('One or more required fields is empty.', 'error');
			}
		}
	},
	activated() {
		eventbus.$on('login-success', message => {
			eventbus.showToast(message, 'success');
			this.$router.go(-1);
		});

		eventbus.$on('login-failure', message => {
			if (this.errorCodes.hasOwnProperty(message))
				eventbus.showToast(this.errorCodes[message], 'error');
			else eventbus.showToast(message, 'error');
		});
	}
};
</script>

<style scoped>
label {
	color: var(--primaryText);
	font-family: 'Roboto', sans-serif;
}

.appContainer {
	padding: 16px;
	padding-top: 50px;
}

.form-control {
	border: solid 0px transparent;
	border-radius: 2px;
	height: 40px;
	font-weight: normal;
}

.form-control:focus {
	box-shadow: 0px 0px 0px transparent;
}

#loadingCircle {
	position: initial;
	margin-left: 20px;
}
</style>

