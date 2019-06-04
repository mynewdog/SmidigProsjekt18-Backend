import React from 'react';
import { Table,TableHead,TableRow,TableCell, TableBody} from "@material-ui/core";
import { grey } from "@material-ui/core/colors";

const styles = {
    addButton: { margin: '1em' },
    removeIcon: { fill: grey },
    lobbycolumns: {
        lobbyname: { width: '40%', fontWeight: 'bold'},
        joinable: { width: '25%', fontWeight: 'bold'},
        members: { width:  '15%', fontWeight: 'bold'},
        messages: { width: '15%', fontWeight: 'bold'}
    },
    usercolumns: {
        connected: { width: '25%' },
        username: { width: '25%' },
        studie: { width: '25%' },
        intitutt: { width: '25%' }
    },
    pagination: { marginTop: '1em' }
};

export const LobbyViewTable = props => { 
        return (

        <Table>
            <TableHead>
                <TableRow>
                    <TableCell style={styles.lobbycolumns.joinable}>Open</TableCell>
                    <TableCell style={styles.lobbycolumns.lobbyname}>Name</TableCell>
                    <TableCell style={styles.lobbycolumns.members}>Members</TableCell>
                    <TableCell style={styles.lobbycolumns.messages}>Messages</TableCell>
                </TableRow>
            </TableHead>
                {props.lobbies.map((lobby,idx) => {
                    return <TableBody key={idx}>
                        <TableRow key={lobby.LobbyName}>
                            <TableCell style={styles.lobbycolumns.joinable}>{lobby.Joinable ? "Yes" : "No"}</TableCell>
                            <TableCell style={styles.lobbycolumns.lobbyname}>{lobby.LobbyName}</TableCell>
                            <TableCell style={styles.lobbycolumns.members}>{lobby.Members.length}</TableCell>
                            <TableCell style={styles.lobbycolumns.messages}>{lobby.Messages.length}</TableCell>
                        </TableRow>
                        <TableRow>
                            <TableCell size="small" style={styles.usercolumns.connected}>Tilkoblet</TableCell>
                            <TableCell size="small" style={styles.usercolumns.username}>Brukernavn</TableCell>
                            <TableCell size="small" style={styles.usercolumns.studie}>Studie</TableCell>
                            <TableCell size="small" style={styles.usercolumns.institutt}>Institutt</TableCell>
                        </TableRow>
                            {lobby.Members.map(item => {
                                return <TableRow key={item.Username}>
                                    <TableCell size="small" style={styles.usercolumns.connected}>{item.Connected ? "Yes" : "No"}</TableCell>
                                    <TableCell size="small" style={styles.usercolumns.username}>{item.Username}</TableCell>
                                    <TableCell size="small" style={styles.usercolumns.studie}>{item.Studie}</TableCell>
                                    <TableCell size="small" style={styles.usercolumns.institutt}>{item.Institutt}</TableCell>
                                </TableRow>
                                }
                            )}
                        </TableBody>
                })}
        </Table>)
}