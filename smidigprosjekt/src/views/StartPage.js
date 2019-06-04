import React from 'react';
import dotnetify from 'dotnetify';
import PropTypes from 'prop-types';
import { withStyles, MuiThemeProvider } from '@material-ui/core/styles';
import ThemeDefault from '../styles/theme-default';
import { UserViewTable } from '../components/UserViewTable';
import { LobbyViewTable } from '../components/LobbyViewTable';

const styles = theme => ({
});
class Statistics extends React.Component {
    constructor(props) {
        super(props);
        this.vm = dotnetify.react.connect('FrontEndManagementVM', this);
        this.state = { 
            // Speiler FrontEndManagementVM.cs
            //Takket være dotnetify :)
            Greetings: '', 
            ServerTime: '',
            TotalLobbies: 0,
            JoinableLobbies: 0,
            UsersInHangout: [],
            Lobbies: [],
            TempLobbies: [],
            Users: []
         };
    }

    componentWillUnmount() {
        this.vm.$destroy();
    }
    render() {
        return (

            <MuiThemeProvider theme={ThemeDefault}>
            <div>
                <h4>SmidigProsjekt</h4>
                <p>{this.state.Greetings}</p>
                <p>Server time is: {this.state.ServerTime}</p>
                <p>Total lobbies: { this.state.TotalLobbies }</p>
                <p>Total active lobbies: {this.state.Lobbies.length}</p>
                <p>Total joinable lobbies: {this.state.JoinableLobbies}</p>
                <p>Total users awaiting lobby: {this.state.UsersInHangout.length}</p>
                <p>Total users: {this.state.Users.length}</p>
            </div>
            <h4>Hangout Activity</h4>
            <UserViewTable users={this.state.UsersInHangout} />
            <h4>Temporary lobbies</h4>
            <LobbyViewTable lobbies={this.state.TempLobbies} />
            <h4>Lobbies</h4>
            <LobbyViewTable lobbies={this.state.Lobbies}/>
            <p>Aktive users</p>
            <UserViewTable users={this.state.Users}/>
            </MuiThemeProvider>
        );
    }
};

export default withStyles(styles)(Statistics);