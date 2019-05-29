import React from 'react';
import dotnetify from 'dotnetify';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';

const styles = theme => ({
});
class Statistics extends React.Component {
    constructor(props) {
        super(props);
        this.vm = dotnetify.react.connect('FrontEndManagementVM', this);
        this.state = { Greetings: '', ServerTime: '' };
    }

    componentWillUnmount() {
        this.vm.$destroy();
    }
    render() {
        return (
            <div>
                <h4>SmidigProsjekt</h4>
                <p>{this.state.Greetings}</p>
                <p>Server time is: {this.state.ServerTime}</p>
                <p>Total lobbies time is: {this.state.TotalLobbies}</p>
                <p>Total users: {this.state.TotalUsers}</p>
            </div>
        );
    }
};

export default withStyles(styles)(Statistics);