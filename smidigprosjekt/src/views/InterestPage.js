import React from 'react';
import dotnetify from 'dotnetify';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';

const styles = theme => ({
});
class InterestPage extends React.Component {
    constructor(props) {
        super(props);
        dotnetify.react.connect('ManageInterestVM', this);
        //this.state = { Greetings: '', ServerTime: '' };
    }

    componentWillUnmount() {
        this.vm.$destroy();
    }
    render() {
        return (
            <div>
                <h4>Interests</h4>
            </div>
        );
    }
};

export default withStyles(styles)(InterestPage);