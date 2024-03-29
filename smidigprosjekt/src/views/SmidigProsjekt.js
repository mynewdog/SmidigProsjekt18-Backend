﻿import React from 'react';
import PropTypes from 'prop-types';
import LoginPage from './LoginPage';
import AppLayout from './AppLayout';
import auth from '../auth';

class SmidigProsjekt extends React.Component {
    constructor(props) {
        super(props);

        this.state = { authenticated: auth.hasAccessToken() };
    }

    render() {
        const handleAuthenticated = _ => this.setState({ authenticated: true });

        return !this.state.authenticated ? <LoginPage onAuthenticated={handleAuthenticated} /> : <AppLayout />;
    }

}

App.propTypes = {
    children: PropTypes.element
};

export default SmidigProsjekt;
