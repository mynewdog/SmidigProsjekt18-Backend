import createMuiTheme from '@material-ui/core/styles/createMuiTheme';
import { blue600, grey900 } from '@material-ui/core/colors';

//Set default colors here
const themeDefault = createMuiTheme({
  typography: {
    useNextVariants: true,
  },
});

export default themeDefault;