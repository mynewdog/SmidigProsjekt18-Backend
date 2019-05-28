import auth from '../auth';
import React from 'react';
import PropTypes from 'prop-types';
import { withStyles } from '@material-ui/core/styles';
import dotnetify from 'dotnetify';;
import Header from '../components/Header';
import Sidebar from '../components/Sidebar';
import ThemeDefault from '../styles/theme-default';
import { MuiThemeProvider } from '@material-ui/core/styles';

const styles = theme => ({
  root: {
    flexGrow: 1,
  },
  content: {
    flexGrow: 1,
    paddingTop: theme.spacing.unit * 10,
    paddingLeft: theme.spacing.unit * 27,
    paddingRight: theme.spacing.unit * 2
  }
});

class AppLayout extends React.Component {
  constructor(props) {

    super(props);

    this.vm = dotnetify.react.connect('AppLayout', this, {
      headers: { Authorization: 'Bearer ' + auth.getAccessToken() },
      exceptionHandler: _ => auth.signOut()
    });


    this.vm.onRouteEnter = (path, template) => (template.Target = 'Content');

    this.state = {
      menuopen: false,
      Menus: []
    };
  }

  componentWillUnmount() {
    this.vm.$destroy();
  }


  render() {
    let { Menus, UserAvatar, UserName, Profile, menuopen } = this.state;
    const userAvatarUrl = UserAvatar ? UserAvatar : null;
    const { classes } = this.props;

    const avatarUrl = Profile ? Profile.AvatarUrl : '';

    const onMenuClick = _ => {
      let currentMenuOpen = menuopen;
      this.setState({ menuopen: !currentMenuOpen });
    }

    return (
      <MuiThemeProvider theme={ThemeDefault}>
        <div className={classes.root}>
                <Header logoTitle="TjommisMGMT" theme={ThemeDefault} username={UserName} avatarUrl={UserAvatar}
            onMenuClick={onMenuClick} />
          <Sidebar vm={this.vm}
            open={menuopen}
            userAvatarUrl={userAvatarUrl}
            menus={Menus}
            theme={ThemeDefault}
          />
          <div id="Content" className={classes.content} />
        </div>
      </MuiThemeProvider>
    );
  }
}

AppLayout.propTypes = {
  classes: PropTypes.object.isRequired,
  menus: PropTypes.arrayOf(PropTypes.string),
  userAvatar: PropTypes.string,
  userName: PropTypes.string,
  width: PropTypes.number
};

export default withStyles(styles)(AppLayout);
