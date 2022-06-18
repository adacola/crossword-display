#!/bin/bash
set -eu

# # マウントされたワークスペースの uid、gid から、
# # vscode ユーザーの uid、gid をセット
# user_id=$(ls -nd /workspaces/* | cut -d ' ' -f 3)
# group_id=$(ls -nd /workspaces/* | cut -d ' ' -f 4)
# sudo groupmod --gid ${group_id} vscode
# sudo usermod --uid ${user_id} --gid ${group_id} vscode

# docker.sock から gid を取得して、docker グループを作成し、
# vscode ユーザーを所属させる
docker_group_id=$(ls -n /var/run/docker.sock | cut -d ' ' -f 4)
sudo groupmod --gid ${docker_group_id} docker

# 無限待ち（vscode デフォルトの entrypoint と同じ方法で）
while sleep 1000; do :; done
