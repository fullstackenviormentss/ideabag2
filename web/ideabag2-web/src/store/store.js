import Vue from 'vue';
import Vuex from 'vuex';

Vue.use(Vuex);

export const store = new Vuex.Store({
	state: {
		categories: [],
		isLoading: true,
		selectedIdeaIndex: 0,
		navbarTitle: 'IdeaBag 2 (BETA)'
	},
	getters: {
		categories: state => {
			return state.categories;
		},
		isLoading: state => {
			return state.isLoading;
		},
		selectedIdeaIndex: state => {
			return state.selectedIdeaIndex;
		},
		navbarTitle: state => {
			return state.navbarTitle;
		}
	},
	mutations: {
		SET_LOADING(state, isLoading) {
			state.isLoading = isLoading;
		},
		SET_CATEGORIES(state, categories) {
			state.categories = categories;
		},
		SET_SELECTED_IDEA_INDEX(state, index) {
			state.selectedIdeaIndex = index;
		},
		SET_TITLE(state, title) {
			state.navbarTitle = title;
		}
	},
	actions: {
		setLoading(context, isLoading) {
			context.commit('SET_LOADING', isLoading);
		},
		setCategories(context, categories) {
			context.commit('SET_CATEGORIES', categories);
		},
		setSelectedIdeaIndex(context, index) {
			context.commit('SET_SELECTED_IDEA_INDEX', index);
		},
		setTitle(context, title) {
			context.commit('SET_TITLE', title);
		}
	}
});
